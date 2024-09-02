using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    [HandleAction(Actions.AddIntervalWeeks)]
    public class AddIntervalWeeksActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public AddIntervalWeeksActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddIntervalWeeksActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var arguments =
                GetAction(update).
                Replace(Actions.AddIntervalWeeks, string.Empty).
                Split("/");
            if (arguments.Length != 2)
            {
                return;
            }
            var jobId = arguments[1];
            if (!int.TryParse(arguments[0], out int weeks))
            {
                return;
            }

            var job = await _jobService.GetAsync(jobId);

            var result = job.LastInterval + weeks;

            if (result < 1 || result > job.Interval)
            {
                return;
            }

            job.LastInterval = result;

            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = await new MessageBuilder(chat).AddJobMenuMarkup(job, _textRepository);
            await msg.AddJobMenuCaption(job, _textRepository);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
