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

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Actions.Dequeue)]
    public class DequeueActionHandler : UpdateHandler
    {
        private QueueService _queueService;
        public DequeueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DequeueActionHandler> logger, QueueService queueService, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
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
            _log.LogInformation("User {id} requested {data}", user.TelegramId, action);
            var actionData = action.Replace(Actions.Dequeue, string.Empty);

            if (!int.TryParse(actionData, out int actionUserId))
            {
                _log.LogError("The id {id} in the chat {chatId} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                // dequeing user
                if (user.TelegramId == actionUserId)
                {
                    if (await _queueService.GetQueueCountAsync(chat.CurrentQueueId) != 1 || chat.Mode == Models.Enums.ChatMode.Open)
                    {
                        await _queueService.DequeueAsync(chat.CurrentQueueId, user.TelegramId);
                        return;
                    }

                    var msg = new MessageBuilder(chat);
                    var result = await _queueService.DequeueAsync(chat.CurrentQueueId, user.TelegramId, false);

                    chat.Mode = Models.Enums.ChatMode.Open;

                    await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, (queue) =>
                    {
                        msg.AddEmptyQueueMarkup(queue.Size, chat.View);
                    });

                    msg
                        .AppendTextLine(TextResources.GetValue(TextKeys.QueueEndedCallingUsers))
                        .AppendTextLine()
                        .AppendText(TextResources.GetValue(TextKeys.CurrentQueue));

                    await DeleteLastMessageAsync(chat);
                    await SendAndUpdateChatAsync(chat, msg, true);
                }
                // swaping two users
                else
                {
                    await _bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Свапи не працюють, сасі", cacheTime: 3);
                    //if (!user.IsAuthorized)
                    //{
                    //    await _bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Необхідно авторизуватись", cacheTime: 3);
                    //    return;
                    //}

                    //await _queueService.SwapUsersAsync(chat.CurrentQueueId, user.TelegramId, actionUserId);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while enqueing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }
        }
    }
}
