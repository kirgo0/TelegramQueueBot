using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Common.Action.ConfirmJobDeletion)]
    public class ConfirmJobDeletionActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public ConfirmJobDeletionActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ConfirmJobDeletionActionHandler> logger, JobService jobService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var jobId = GetAction(update).Replace(Common.Action.ConfirmJobDeletion, string.Empty);
            await _jobService.DeleteJobAsync(jobId);
            await base.RedirectHandle(
                update,
                Metatags.HandleCommand,
                (value) => value.Equals(Command.Jobs),
                "An error ocured while redirecting from {from} action handler to the {to} command handler",
                Common.Action.ConfirmJobDeletion, Command.Jobs
                );
        }
    }
}

