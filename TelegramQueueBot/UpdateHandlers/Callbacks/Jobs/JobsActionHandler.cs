using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.Jobs)]
    public class JobsActionHandler : UpdateHandler
    {
        public JobsActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobsActionHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (value) => value.Equals(Command.Jobs),
                "An error ocured while redirecting from {from} action handler to the {to} command handler",
                Actions.Jobs, Command.Jobs
                );
        }
    }
}
