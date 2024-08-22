using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
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
        public GetCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<GetCommandHandler> logger, QueueService queueService, IUserRepository userRepository, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg); 
                return; 
            }
            var queue = await _queueService.GetQueueSnapshotAsync(chat.CurrentQueueId);
            if (queue is null)
            {
                _log.LogError("An error occurred while retrieving a queue from the repository for chat {id}", chat.TelegramId);
                return;
            }

            var names = await _userRepository.GetRangeByTelegramIdsAsync(queue.List);

            await msg.AppendModeTitle(chat, _textRepository);
            msg
                .AppendText(await _textRepository.GetValueAsync(TextKeys.CurrentQueue))
                .AddDefaultQueueMarkup(names);

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg);
        }
    }
}
