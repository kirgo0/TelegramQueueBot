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
    [HandleAction(Common.Action.SetInterval)]
    public class SetIntervalActionHandler : ModifyJobActionHandler<int>
    {
        public SetIntervalActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetIntervalActionHandler> logger, JobService jobService, QueueService queueService) : base(bot, scope, logger, jobService, queueService)
        {
        }
        public override bool ActionWithJob(ChatJob job, int data)
        {
            if (job.Interval == data)
            {
                return false;
            }
            job.Interval = data;
            if (job.LastInterval > job.Interval)
            {
                job.LastInterval = job.Interval;
            }
            return true;
        }

        public override bool ParseActionParameter(string parameter, out int data)
        {
            return int.TryParse(parameter, out data);
        }
    }
}

