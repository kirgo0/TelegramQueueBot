using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{

    [HandleAction(Common.Action.DeleteJob)]
    public class DeleteJobActionHandler : UpdateHandler
    {
        public DeleteJobActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DeleteJobActionHandler> logger) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var jobId = GetAction(update).Replace(Common.Action.DeleteJob, string.Empty);
            var chat = await chatTask;
            var msg = new MessageBuilder(chat)
                .AppendText($"{TextResources.GetValue(TextKeys.ConfirmJobDeletion)}")
                .AddButton(TextResources.GetValue(TextKeys.BackBtn), $"{Common.Action.JobMenu}{jobId}")
                .AddButton(TextResources.GetValue(TextKeys.ConfirmDeletionBtn), $"{Common.Action.ConfirmJobDeletion}{jobId}");

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
