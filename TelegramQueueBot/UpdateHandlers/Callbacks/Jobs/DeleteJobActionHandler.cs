using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{

    [HandleAction(Actions.DeleteJob)]
    public class DeleteJobActionHandler : UpdateHandler
    {
        public DeleteJobActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DeleteJobActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            var jobId = GetAction(update).Replace(Actions.DeleteJob, string.Empty);
            msg
                .AppendText($"{TextResources.GetValue(TextKeys.ConfirmJobDeletion)}")
                .AddButton(TextResources.GetValue(TextKeys.BackBtn), $"{Actions.JobMenu}{jobId}")
                .AddButton(TextResources.GetValue(TextKeys.ConfirmDeletionBtn), $"{Actions.ConfirmJobDeletion}{jobId}");

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
