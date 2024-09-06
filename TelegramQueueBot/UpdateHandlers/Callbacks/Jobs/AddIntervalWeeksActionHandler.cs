using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using TelegramQueueBot.UpdateHandlers.Callbacks.Jobs.Abstract;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.AddIntervalWeeks)]
    public class AddIntervalWeeksActionHandler : ModifyJobActionHandler<int>
    {
        public AddIntervalWeeksActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddIntervalWeeksActionHandler> logger, JobService jobService) : base(bot, scope, logger, jobService)
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
