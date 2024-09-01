using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.ConfirmJobDeletion)]
    public class ConfirmJobDeletionActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public ConfirmJobDeletionActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ConfirmJobDeletionActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var jobId = GetAction(update).Replace(Actions.ConfirmJobDeletion, string.Empty);
            await _jobService.DeleteJobAsync(jobId);
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) => value.Equals(Command.Jobs),
                "An error ocured while redirecting from {from} action handler to the {to} command handler",
                Actions.ConfirmJobDeletion, Command.Jobs
                );
        }
    }
}

