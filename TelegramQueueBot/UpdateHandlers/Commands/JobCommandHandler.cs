using Autofac;
using Cronos;
using Hangfire.Mongo;
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

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandlesCommand(Command.Job)]
    public class JobCommandHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public JobCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobCommandHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if(string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            var now = DateTime.UtcNow;
            //var minutes = now.Minute % 5 == 0 ? now.Minute + 5 : now.Minute + (5 - now.Minute % 5);
            var minutes = now.Minute + 1;
            var cron = $"{minutes} {now.Hour} * * {(int)now.DayOfWeek}";
            var job = await _jobService.CreateJobAsync(chat.TelegramId, "New job", cron);


        }
    }
}
