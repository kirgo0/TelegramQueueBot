﻿using Autofac;
using Hangfire;
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
    [HandleAction(Actions.AddDays)]
    public class AddDaysActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public AddDaysActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AddDaysActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var arguments = 
                GetAction(update).
                Replace(Actions.AddDays, string.Empty).
                Split("/");

            if (arguments.Length != 2)
            {
                return;
            }
            var jobId = arguments[1];
            if (!int.TryParse(arguments[0], out int days))
            {
                return;
            }
            var job = await _jobService.GetAsync(jobId);
            job.CronExpression = CronHelper.AddDays(job.CronExpression, days);
            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = await new MessageBuilder(chat).AddJobMenuMarkup(job.Id, job.NextRunTime, _textRepository);
            await msg.AddJobMenuCaption(job, _textRepository);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}