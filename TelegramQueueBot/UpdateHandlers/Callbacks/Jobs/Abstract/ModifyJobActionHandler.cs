using Autofac;
using Hangfire.Common;
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
using TelegramQueueBot.Models;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs.Abstract
{
    public abstract class ModifyJobActionHandler<T> : UpdateHandler
    {
        private readonly JobService _jobService;
        public ModifyJobActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger, JobService jobService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobService = jobService;
        }

        public virtual string GetAction()
        {
            var type = GetType();

            var attribute = (HandleActionAttribute)Attribute.GetCustomAttribute(type, typeof(HandleActionAttribute));

            if (attribute is not null)
            {
                return attribute.Value.ToString();
            }
            throw new Exception($"No {nameof(HandleActionAttribute)} was found to parse action");
        }

        public override async Task Handle(Update update)
        {
            var arguments =
                GetAction(update).
                Replace(GetAction(), string.Empty).
                Split("/");

            if (arguments.Length != 2)
            {
                return;
            }

            if(!ParseActionParameter(arguments[0], out T actionData))
            {
                return;
            }

            var jobId = arguments[1];
            var job = await _jobService.GetAsync(jobId);

            var result = ActionWithJob(job, actionData);

            if(!result) { return; }

            await _jobService.UpdateJobAsync(job);

            var chat = await chatTask;
            var msg = new MessageBuilder(chat)
                .AddJobMenuCaption(job)
                .AddJobMenuMarkup(job);

            await _bot.BuildAndEditAsync(msg);
        }

        public abstract bool ActionWithJob(ChatJob job, T data);

        public abstract bool ParseActionParameter(string parameter, out T data);
    }
}
