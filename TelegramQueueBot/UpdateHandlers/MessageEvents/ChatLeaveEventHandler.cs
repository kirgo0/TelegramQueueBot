using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.MessageEvents
{
    [HandlerMetadata(Metatags.HandleMessageEvent, nameof(Update.Message.LeftChatMember))]
    public class ChatLeaveEventHandler : UpdateHandler
    {
        public ChatLeaveEventHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ChatLeaveEventHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            var userName = update.Message.LeftChatMember.Username;
            if (!userName.Equals(botName))
            {
                return;
            }
            _log.LogInformation("Leaved chat");
            //var chat = await TryGetOrCreateChat(_chatId);
            // TODO: delete all chat & users data
        }
    }
}
