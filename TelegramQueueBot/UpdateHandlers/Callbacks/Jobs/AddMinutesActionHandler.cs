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
    [HandleAction(Actions.AddMinutes)]
    public class AddMinutesActionHandler : ModifyJobActionHandler<int>
    {
        public AddMinutesActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddMinutesActionHandler> logger, JobService jobService) : base(bot, scope, logger, jobService)
        {
        }

        //private readonly JobService _jobService;
        //public AddMinutesActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddMinutesActionHandler> logger,  JobService jobService) : base(bot, scope, logger)
        //{
        //    GroupsOnly = true;
        //    NeedsChat = true;
        //    _jobService = jobService;
        //}

        //public override async Task Handle(Update update)
        //{
        //    var arguments =
        //        GetAction(update).
        //        Replace(Actions.AddMinutes, string.Empty).
        //        Split("/");
        //    if (arguments.Length != 2)
        //    {
        //        return;
        //    }
        //    var jobId = arguments[1];
        //    if (!int.TryParse(arguments[0], out int minutes))
        //    {
        //        return;
        //    }

        //    var job = await _jobService.GetAsync(jobId);

        //    job.CronExpression = CronHelper.AddMinutes(job.CronExpression, minutes);
        //    await _jobService.UpdateJobAsync(job);

        //    var chat = await chatTask;
        //    var msg = new MessageBuilder(chat)
        //        .AddJobMenuMarkup(job)
        //        .AddJobMenuCaption(job);

        //    await _bot.BuildAndEditAsync(msg);

        //}
        public override bool ActionWithJob(ChatJob job, int data)
        {
            try
            {
                job.CronExpression = CronHelper.AddMinutes(job.CronExpression, data);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error ocured while adding minutes {minutes} to the cron expression {cronExpression}", data, job.CronExpression);
                return false;
            }
        }

        public override bool ParseActionParameter(string parameter, out int data)
        {
            return int.TryParse(parameter, out data);
        }
    }
}

