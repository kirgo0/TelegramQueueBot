using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class DequeueActionHandler : UpdateHandler
    {
        private QueueService _queueService;
        public DequeueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DequeueActionHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            NeedsUser = true;
            NeedsChat = true;
            GroupsOnly = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var user = await userTask;
            _log.LogInformation("User {id} requested {data}", update.CallbackQuery.From.Id, update.CallbackQuery.Data);
            var action = GetAction(update);
            var actionData = action.Replace(Actions.Dequeue, string.Empty);
            if (!int.TryParse(actionData, out int actionUserId))
            {
                _log.LogError("The id {id} in the chat {chatId} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                if (user.TelegramId == actionUserId)
                    await _queueService.DequeueAsync(chat.CurrentQueueId, user.TelegramId);
                else
                {
                    if (!user.IsAuthorized)
                    {
                        await _bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Необхідно авторизуватись", cacheTime: 3);
                        return;
                    }

                    await _queueService.SwapUsersAsync(chat.CurrentQueueId, user.TelegramId, actionUserId);

                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while enqueing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }
        }
    }
}
