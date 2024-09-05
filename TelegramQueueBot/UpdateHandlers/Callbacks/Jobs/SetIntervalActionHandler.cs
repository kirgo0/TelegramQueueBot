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
    [HandleAction(Actions.SetInterval)]
    public class SetIntervalActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public SetIntervalActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetIntervalActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var arguments =
                GetAction(update).
                Replace(Actions.SetInterval, string.Empty).
                Split("/");

            if (arguments.Length != 2)
            {
                return;
            }
            if (!int.TryParse(arguments[0], out var interval))
            {
                return;
            }
            var jobId = arguments[1];

            var job = await _jobService.GetAsync(jobId);
            if (job.Interval == interval)
            {
                return;
            }
            job.Interval = interval;
            if (job.LastInterval > job.Interval) job.LastInterval = job.Interval;
            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            await msg.AddJobMenuCaption(job);
            await msg.AddJobMenuMarkup(job);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}

