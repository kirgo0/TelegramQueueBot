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
        public AddMinutesActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddMinutesActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
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
            var msg = await new MessageBuilder(chat).AddJobMenuMarkup(job, _textRepository);
            await msg.AddJobMenuCaption(job, _textRepository);

            await _bot.BuildAndEditAsync(msg);

        }
    }
}

