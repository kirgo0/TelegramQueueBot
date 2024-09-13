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
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UserNotifyingUpdateHandler : UpdateHandler
    {
        protected UserNotifyingUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
        }
        protected async Task NotifyUsersIfOrderChanged(Chat chat, List<long> previousOrder, List<long> currentOrder)
        {

            var notifyTasks = new List<Task>();

            for (int i = 0; i < currentOrder.Count; i++)
            {
                long userId = (previousOrder.Count - 1 >= i && currentOrder[i] != previousOrder[i]) || previousOrder.Count - 1 < i
                    ? currentOrder[i]
                    : 0;
                if (i == 0 && userId != 0)
                {
                    var leaveBtn = new InlineKeyboardButton(TextResources.GetValue(TextKeys.LeaveBtn));
                    leaveBtn.CallbackData = $"{Common.Action.Leave}{chat.TelegramId}";
                    notifyTasks.Add(NotifyUserAsync(
                        $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{TextResources.GetValue(TextKeys.FirstUserInQueue)}", 
                        chat.Id, 
                        userId, 
                        markup: new InlineKeyboardMarkup(leaveBtn)
                        ));
                }
                else if (userId != 0)
                {
                    notifyTasks.Add(
                        NotifyUserAsync(
                            $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{string.Format(TextResources.GetValue(TextKeys.NextUserInQueue), i + 1)}",
                            chat.Id, userId)
                    );
                }
            }
            await Task.WhenAll(notifyTasks);

        }

        protected async Task NotifyUserAsync(string message, string chatId, long userId, ParseMode parseMode = ParseMode.Html, InlineKeyboardMarkup markup = null)
        {
            var user = await _userRepository.GetByTelegramIdAsync(userId);
            if (!(user).SendNotifications) return;
            if (!user.ChatIds.TryGetValue(chatId, out bool enabled) || !enabled) return;

            await _bot.SendTextMessageAsync(
                userId,
                message,
                parseMode: parseMode,
                replyMarkup: markup
            );
        }

        protected async Task SendUserMessageAsync(long userId, MessageBuilder messageTemplate)
        {
            messageTemplate.SetChatId(userId);
            await _bot.BuildAndSendAsync(messageTemplate);
        }

    }
}
