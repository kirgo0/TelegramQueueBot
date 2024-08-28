using Autofac;
using Hangfire;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.JobMenu)]
    public class JobMenuActionHandler : UpdateHandler
    {
        private readonly IBackgroundJobClient _jobClient;
        public JobMenuActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobMenuActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
