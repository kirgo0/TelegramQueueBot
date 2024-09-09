using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Callbacks.Jobs.Abstract;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Common.Action.SetQueue)]
    public class SetQueueActionHandler : ModifyJobActionHandler<string>
    {
        public SetQueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetQueueActionHandler> logger, JobService jobService, QueueService queueService) : base(bot, scope, logger, jobService, queueService)
        {
        }
        public override bool ActionWithJob(ChatJob job, string data)
        {
            if (job.QueueId is not null && job.QueueId.Equals(data)) return false;
            job.QueueId = string.IsNullOrEmpty(data) ? null : data;
            return true;
        }

        public override bool ParseActionParameter(string parameter, out string data)
        {
            data = parameter;
            return true;
        }
    }
}
