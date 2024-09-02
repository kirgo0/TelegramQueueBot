using Autofac;
using CronExpressionDescriptor;
using Cronos;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Globalization;
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
    [HandleAction(Actions.JobMenu)]
    public class JobMenuActionHandler : UpdateHandler
    {
        private readonly JobService _jobService;
        public JobMenuActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobMenuActionHandler> logger, ITextRepository textRepository, JobService jobService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            var data = GetAction(update).Replace(Actions.JobMenu, "");
            var job = await _jobService.GetAsync(data); 

            await msg.AddJobMenuCaption(job, _textRepository);
            await msg.AddJobMenuMarkup(job, _textRepository);

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
