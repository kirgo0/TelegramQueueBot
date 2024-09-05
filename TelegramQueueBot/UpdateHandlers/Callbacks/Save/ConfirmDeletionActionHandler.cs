using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Save
{
    [HandleAction(Actions.ConfirmDeletion)]
    public class ConfirmDeletionActionHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public ConfirmDeletionActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ConfirmDeletionActionHandler> logger,  QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var queueId = GetAction(update).Replace(Actions.ConfirmDeletion, string.Empty);

            var result = await _queueService.DeleteQueueAsync(queueId);
            if (!result)
            {
                _log.LogError("An error occured while deleting a queue with id {queueId}", queueId);
                return;
            }

            chat.SavedQueuesIds.Remove(queueId);
            if (chat.CurrentQueueId.Equals(queueId)) chat.CurrentQueueId = string.Empty;
            await _chatRepository.UpdateAsync(chat);

            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) => value.Equals(Command.SavedList),
                "An error ocured while redirecting from {from} to {to}",
                Actions.ConfirmDeletion, Command.SavedList
                );
        }
    }
}
