using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.JobMenu)]
    public class JobMenuActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        private readonly QueueService _queueService;
        public JobMenuActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobMenuActionHandler> logger, JobService jobService, QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var data = GetAction(update).Replace(Actions.JobMenu, string.Empty);
            var job = await _jobService.GetAsync(data);

            if (job.QueueId is not null)
            {
                var queue = await _queueService.GetByIdAsync(job.QueueId);
                if (queue is null)
                {
                    job.QueueId = null;
                    await _jobService.UpdateJobAsync(job);
                }
            }

            var chat = await chatTask;
            string queueName = string.Empty;
            if (job.QueueId is not null)
            {
                queueName = (await _queueService.GetByIdAsync(job.QueueId)).Name;
            }
            var msg = new MessageBuilder(chat)
                .AddJobMenuCaption(job)
                .AddJobMenuMarkup(job, queueName);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
