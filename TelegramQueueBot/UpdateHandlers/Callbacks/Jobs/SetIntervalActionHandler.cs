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
    [HandleAction(Actions.SetInterval)]
    public class SetIntervalActionHandler : ModifyJobActionHandler<int>
    {
        public SetIntervalActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetIntervalActionHandler> logger, JobService jobService) : base(bot, scope, logger, jobService)
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

