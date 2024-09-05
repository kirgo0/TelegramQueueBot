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
    [HandlesCommand(Command.Create)]
    public class CreateCommandHandler : UpdateHandler
    {
        private QueueService _queueService;

        public CreateCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CreateCommandHandler> logger, QueueService queueService ) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            if (!string.IsNullOrEmpty(chat.CurrentQueueId))
                await DeleteLastMessageAsync(chat);

            var queue = await _queueService.CreateQueueAsync(chat.TelegramId, chat.DefaultQueueSize);
            if (queue is null)
            {
                _log.LogWarning("An error occurred when creating a queue for chat {id}, a null value was received", chat.TelegramId);
                return;
            }
            if (!string.IsNullOrEmpty(chat.CurrentQueueId) && !chat.SavedQueuesIds.Contains(chat.CurrentQueueId))
                await _queueService.DeleteQueueAsync(chat.CurrentQueueId);

            chat.CurrentQueueId = queue.Id;
            var msg = new MessageBuilder(chat)
                .AppendTextLine(TextResources.GetValue(TextKeys.CreatedQueue))
                .AddDefaultQueueMarkup(new List<Models.User>(new Models.User[chat.DefaultQueueSize]), chat.View);

            await SendAndUpdateChatAsync(chat, msg, true);
        }
    }
}
