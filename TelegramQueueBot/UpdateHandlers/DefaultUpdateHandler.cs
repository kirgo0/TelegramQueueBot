using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    public class DefaultUpdateHandler : UpdateHandler
    {
        public DefaultUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DefaultUpdateHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update, 
                Metatags.HandleType, 
                (udpate, type, item) => (UpdateType)type == update.Type,
                "An error occured while resolving update handler for type {type}",
                update.Type
            );
        }
    }
}
