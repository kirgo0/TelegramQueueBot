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

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandlesCommand(Command.Job)]
    public class JobCommandHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public JobCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobCommandHandler> logger,  JobService jobService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(TextResources.GetValue(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            var now = DateTime.UtcNow;
            now = now.AddMinutes(1);
            //var minutes = now.Minute % 5 == 0 ? now.Minute + 5 : now.Minute + (5 - now.Minute % 5);
            var cron = $"{now.Minute} {now.Hour} * * {(int)now.DayOfWeek}";
            var job = await _jobService.CreateJobAsync(chat.TelegramId, "New job", cron);

            msg.AddJobMenuCaption(job);
            msg.AddJobMenuMarkup(job);

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg);
        }
    }
}
