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
    [HandleAction(Actions.SetQueue)]
    public class SetQueueActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public SetQueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetQueueActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var arguments =
                GetAction(update).
                Replace(Actions.SetQueue, string.Empty).
                Split("/");
            if (arguments.Length != 2)
            {
                return;
            }
            var queueId = arguments[0];
            var jobId = arguments[1];

            var job = await _jobService.GetAsync(jobId);
            job.QueueId = string.IsNullOrEmpty(queueId) ? null : queueId;
            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            await msg.AddJobMenuCaption(job);
            await msg.AddJobMenuMarkup(job);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
