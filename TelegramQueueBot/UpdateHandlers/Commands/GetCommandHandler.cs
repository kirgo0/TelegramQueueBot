using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class GetCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public GetCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<GetCommandHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            if (string.IsNullOrEmpty(chat.CurrentQueueId)) return; // TODO: no queue created message
            var queue = await _queueService.GetQueueSnapshotAsync(chat.CurrentQueueId);
            if (queue is null)
            {
                _log.LogError("An error occurred while retrieving a queue from the repository for chat {id}", chat.TelegramId);
                return;
            }
            var deleteTask = DeleteLastMessageAsync(chat);
            var names = await _userRepository.GetRangeByTelegramIdsAsync(queue.List);
            var msg = new MessageBuilder(chat)
                .AppendText("Ну тіпа пасасав")
                .AddDefaultQueueMarkup(names);

            await deleteTask;
            var response = await _bot.BuildAndSendAsync(msg);
            if (response is not null)
            {
                chat.LastMessageId = response.MessageId;
                await _chatRepository.UpdateAsync(chat);
            }
        }
    }
}
