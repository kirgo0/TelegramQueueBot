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
    [HandleAction(Actions.JobQueueMenu)]
    public class JobQueueMenuActionHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public JobQueueMenuActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobMenuActionHandler> logger, ITextRepository textRepository, QueueService queueService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if (!chat.SavedQueuesIds.Any())
            {
                msg.AppendText(TextResources.GetValue(TextKeys.NoSavedQueues));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            var queues = await _queueService.GetByIdsAsync(chat.SavedQueuesIds);

            if (!queues.Any())
            {
                chat.SavedQueuesIds = new List<string>();
                msg.AppendText(TextResources.GetValue(TextKeys.NoSavedQueues));
                await SendAndUpdateChatAsync(chat, msg, true);
                return;
            }

            var jobId = GetAction(update).Replace(Actions.JobQueueMenu, string.Empty);
            msg.AppendText(TextResources.GetValue(TextKeys.SelectJobQueueMenu));

            msg.AddButtonNextRow("Пуста черга", $"{Actions.SetQueue}/{jobId}");
            foreach (var queue in queues)
            {
                msg.AddButtonNextRow(queue.Name, $"{Actions.SetQueue}{queue.Id}/{jobId}");
            }

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
