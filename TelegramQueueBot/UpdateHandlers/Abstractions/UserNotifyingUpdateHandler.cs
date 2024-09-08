using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UserNotifyingUpdateHandler : UpdateHandler
    {
        protected UserNotifyingUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
        }
        protected async Task NotifyUsersIfOrderChanged(long chatId, List<long> previousOrder, List<long> currentOrder)
        {

            var notifyTasks = new List<Task>();

            for (int i = 0; i < currentOrder.Count; i++)
            {
                long userId = (previousOrder.Count - 1 >= i && currentOrder[i] != previousOrder[i]) || previousOrder.Count - 1 < i
                    ? currentOrder[i]
                    : 0;
                if (i == 0 && userId != 0)
                {
                    notifyTasks.Add(NotifyUserAsync(
                        $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{TextResources.GetValue(TextKeys.FirstUserInQueue)}"
                        , chatId, userId));
                }
                else if (userId != 0)
                {
                    notifyTasks.Add(
                        NotifyUserAsync(
                            $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{string.Format(TextResources.GetValue(TextKeys.NextUserInQueue), i + 1)}",
                            chatId, userId)
                    );
                }
            }
            await Task.WhenAll(notifyTasks);

        }

        protected async Task NotifyUserAsync(string message, long chatId, long userId, ParseMode parseMode = ParseMode.Html, InlineKeyboardMarkup markup = null)
        {
            var user = await _userRepository.GetByTelegramIdAsync(userId);
            if (!(user).SendNotifications) return;
            if (!user.AllowedNotificationChatIds.Contains(chatId)) return;

            await _bot.SendTextMessageAsync(
                userId,
                message,
                parseMode: parseMode,
                replyMarkup: markup
            );
        }

    }
}
