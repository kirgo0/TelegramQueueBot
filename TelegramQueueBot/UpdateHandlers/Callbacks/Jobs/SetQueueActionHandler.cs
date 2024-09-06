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
    [HandleAction(Actions.SetQueue)]
    public class SetQueueActionHandler : ModifyJobActionHandler<string>
    {
        public SetQueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetQueueActionHandler> logger, JobService jobService) : base(bot, scope, logger, jobService)
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
