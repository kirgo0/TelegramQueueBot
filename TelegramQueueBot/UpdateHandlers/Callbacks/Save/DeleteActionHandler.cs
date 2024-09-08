using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Save
{
    [HandleAction(Actions.Delete)]
    public class DeleteActionHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public DeleteActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DeleteActionHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            var queueId = GetAction(update).Replace(Actions.Delete, string.Empty);
            var queue = await _queueService.GetByIdAsync(queueId);
            if(queue is null)
            {
                // TODO: Exception
                return;
            }
            var queueName = queue.Name;
            msg
                .AppendTextFormat(TextResources.GetValue(TextKeys.ConfirmDeletion), queueName)
                .AddButton(TextResources.GetValue(TextKeys.BackBtn), $"{Actions.QueueMenu}{queueId}")
                .AddButton(TextResources.GetValue(TextKeys.ConfirmDeletionBtn), $"{Actions.ConfirmDeletion}{queueId}");

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
