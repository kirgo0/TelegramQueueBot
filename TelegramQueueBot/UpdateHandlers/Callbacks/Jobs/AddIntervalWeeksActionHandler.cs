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
    [HandleAction(Actions.AddIntervalWeeks)]
    public class AddIntervalWeeksActionHandler : ModifyJobActionHandler<int>
    {
        public AddIntervalWeeksActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddIntervalWeeksActionHandler> logger, JobService jobService, QueueService queueService) : base(bot, scope, logger, jobService, queueService)
        {
        }

        public override bool ActionWithJob(ChatJob job, int data)
        {
            var result = job.LastInterval + data;

            if (result < 1 || result > job.Interval)
            {
                return false;
            }

            job.LastInterval = result;
            return true;
        }

        public override bool ParseActionParameter(string parameter, out int data)
        {
            return int.TryParse(parameter, out data);
        }
    }
}
