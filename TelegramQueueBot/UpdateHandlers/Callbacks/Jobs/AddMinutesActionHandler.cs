using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.AddMinutes)]
    public class AddMinutesActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public AddMinutesActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddMinutesActionHandler> logger,  JobService jobService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var arguments =
                GetAction(update).
                Replace(Actions.AddMinutes, string.Empty).
                Split("/");
            if (arguments.Length != 2)
            {
                return;
            }
            var jobId = arguments[1];
            if (!int.TryParse(arguments[0], out int minutes))
            {
                return;
            }

            var job = await _jobService.GetAsync(jobId);

            job.CronExpression = CronHelper.AddMinutes(job.CronExpression, minutes);

            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = new MessageBuilder(chat).AddJobMenuMarkup(job);
            msg.AddJobMenuCaption(job);

            await _bot.BuildAndEditAsync(msg);

        }
    }
}

