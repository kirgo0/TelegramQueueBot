using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Save
{
    [HandleAction(Actions.QueueList)]
    public class QueueListActionHandler : UpdateHandler
    {
        public QueueListActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<QueueListActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) => value.Equals(Command.SavedList),
                "An error ocured while redirecting from {from} to {to}",
                Actions.QueueList, Command.SavedList
                );

        }
    }
}
