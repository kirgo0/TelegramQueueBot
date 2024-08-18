using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class CreateCommandHandler : UpdateHandler
    {
        private QueueService _queueService;

        public CreateCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CreateCommandHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            _queueService = queueService;

            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var queue = await _queueService.CreateQueueAsync(chat.TelegramId, chat.DefaultQueueSize);
            if (queue is null)
            {
                _log.LogWarning("An error occurred when creating a queue for chat {id}, a null value was received", chat.TelegramId);
                return;
            }
            chat.CurrentQueueId = queue.Id;

            await _chatRepository.UpdateAsync(chat);

            var msg = new MessageBuilder(chat)
                .AppendText("Ну тіпа сасі")
                .AddDefaultQueueMarkup(new List<Models.User>(new Models.User[chat.DefaultQueueSize]));

            await _bot.BuildAndSendAsync(msg);
        }
    }
}
