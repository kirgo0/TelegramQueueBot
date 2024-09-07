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
    [HandlesCommand(Command.Notify)]
    public class NotifyCommandHandler : UpdateHandler
    {
        public NotifyCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<NotifyCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
        }

        public override async Task Handle(Update update)
        {
            var user = await userTask;
            user.SendNotifications = !user.SendNotifications;
            var msg = new MessageBuilder()
                .SetChatId(user.TelegramId)
                .AppendText(TextResources.GetValue(user.SendNotifications ? TextKeys.UserRecievingNotifications : TextKeys.UserNotRecievingNotifications));
            await _userRepository.UpdateAsync(user);
            await _bot.BuildAndSendAsync(msg);
        }
    }
}
