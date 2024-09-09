using Autofac;
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
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandleCommand(Command.Notify)]
    public class NotifyCommandHandler : UpdateHandler
    {
        public NotifyCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<NotifyCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var user = await userTask;
            var chat = await chatTask;
            var msg = new MessageBuilder()
                .SetChatId(user.TelegramId);
            // private messages 
            if (chat is null)
            {
                // enable/disable notifications globally
                user.SendNotifications = !user.SendNotifications;
                msg.AppendText(TextResources.GetValue(user.SendNotifications ? TextKeys.UserRecievingNotifications : TextKeys.UserNotRecievingNotifications));
            }
            else
            {
                // enable/disable notifications for chat
                var chatNameTask = _bot.GetChatAsync(chat.TelegramId);
                var isEnabled = user.AllowedNotificationChatIds.Contains(chat.TelegramId);
                if (isEnabled)
                {
                    user.AllowedNotificationChatIds.Remove(chat.TelegramId);
                    msg.AppendTextFormat(TextResources.GetValue(TextKeys.DisableChatNotifications), (await chatNameTask).Title);
                }
                else
                {
                    user.AllowedNotificationChatIds.Add(chat.TelegramId);
                    msg.AppendTextFormat(TextResources.GetValue(TextKeys.EnableChatNotifications), (await chatNameTask).Title);
                }
            }
            await _userRepository.UpdateAsync(user);
            await _bot.BuildAndSendAsync(msg);

        }
    }
}
