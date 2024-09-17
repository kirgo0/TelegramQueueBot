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
        private IConfiguration _configuration;
        public ChatJoinEventHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ChatJoinEventHandler> logger, IConfiguration configuration) : base(bot, scope, logger)
        {
            _configuration = configuration;
        }

        public override async Task Handle(Update update)
        {
            var userName = update.Message.NewChatMembers[0].Username;
            if (!userName.Equals(_configuration.GetSection("TelegramBotOptions")["BotName"]))
            {
                return;
            }
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
