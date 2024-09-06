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

namespace TelegramQueueBot.UpdateHandlers.Commands.Job
{
    [HandlesCommand(Command.Job)]
    public class JobCommandHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public JobCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobCommandHandler> logger, JobService jobService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            var nextOccurence = RoundUpToNextFiveMinutes(DateTime.UtcNow);
            var cron = $"{nextOccurence.Minute} {nextOccurence.Hour} * * {(int)nextOccurence.DayOfWeek}";
            var job = await _jobService.CreateJobAsync(chat.TelegramId, "New job", cron);

            msg.AddJobMenuCaption(job);
            msg.AddJobMenuMarkup(job);

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg);
        }

        private DateTime RoundUpToNextFiveMinutes(DateTime dateTime)
        {
            int minutes = dateTime.Minute;
            int extraMinutes = 5 - (minutes % 5);
            return dateTime.AddMinutes(extraMinutes == 5 ? 5 : extraMinutes).AddSeconds(-dateTime.Second).AddMilliseconds(-dateTime.Millisecond);
        }
    }
}
