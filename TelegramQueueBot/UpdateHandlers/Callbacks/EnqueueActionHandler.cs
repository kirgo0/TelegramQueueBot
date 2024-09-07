using Autofac;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Actions.Enqueue)]
    public class EnqueueActionHandler : UpdateHandler
    {
        private QueueService _queueService;
        public EnqueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<EnqueueActionHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsUser = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var user = await userTask;
            var action = GetAction(update);
            _log.LogInformation("User {id} from chat {chatId} requested {data}", user.TelegramId, chat.TelegramId, action);

            var actionData = action.Replace(Actions.Enqueue, string.Empty);
            if (!int.TryParse(actionData, out int pos))
            {
                _log.LogError("The position {pos} in the chat {id} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                if (chat.Mode is not Models.Enums.ChatMode.CallingUsers)
                {
                    await _queueService.EnqueueAsync(chat.CurrentQueueId, pos, user.TelegramId);
                }
                else
                {
                    var firstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
                    await _queueService.EnqueueAsync(chat.CurrentQueueId, pos, user.TelegramId);
                    var nextfirstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
                    await NotifyUsersIfOrderChanged(firstTwoUsers, nextfirstTwoUsers);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while enqueing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }

        }

        private async Task NotifyUsersIfOrderChanged(List<long> previousOrder, List<long> currentOrder)
        {

            var notifyTasks = new List<Task>();

            for (int i = 0; i < currentOrder.Count; i++)
            {
                long userId = (previousOrder.Count - 1 >= i && currentOrder[i] != previousOrder[i]) || previousOrder.Count - 1 < i
                    ? currentOrder[i]
                    : 0;
                if (i == 0 && userId != 0)
                {
                    notifyTasks.Add(NotifyUserAsync(TextResources.GetValue(TextKeys.FirstUserInQueue), userId));
                }
                else if (userId != 0)
                {
                    notifyTasks.Add(
                        NotifyUserAsync(
                            string.Format(TextResources.GetValue(TextKeys.NextUserInQueue), i + 1),
                            userId)
                    );
                }
            }

            await Task.WhenAll(notifyTasks);

        }
    }
}
