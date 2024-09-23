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
    [HandlerMetadata(Metatags.HandleMessageEvent, nameof(Update.Message.NewChatMembers))]
    public class ChatJoinEventHandler : UpdateHandler
    {
        public ChatJoinEventHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ChatJoinEventHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            var userName = update.Message.NewChatMembers[0].Username;
            if (!userName.Equals(botName))
            {
                return;
            }
            _log.LogInformation("Bot has joined a chat with id {telegramid}", update.Message.Chat.Id);
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (value) => value.Equals(Command.Start),
                "An error occured while redirecting from {from} to {to}",
                nameof(Update.Message.NewChatMembers), Command.Start
                );
        }
    }
}
