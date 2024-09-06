using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Callbacks.Jobs.Abstract;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.AddDays)]
    public class AddDaysActionHandler : ModifyJobActionHandler<int>
    {

        public AddDaysActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddDaysActionHandler> logger, JobService jobService, QueueService queueService) : base(bot, scope, logger, jobService, queueService)
        {
        }

        public override bool ActionWithJob(ChatJob job, int data)
        {
            try
            {
                job.CronExpression = CronHelper.AddDays(job.CronExpression, data);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error ocured while adding days {days} to the cron expression {cronExpression}", data, job.CronExpression);
                return false;
            }
        }

        public override bool ParseActionParameter(string parameter, out int data)
        {
            return int.TryParse(parameter, out data);
        }
    }
}
