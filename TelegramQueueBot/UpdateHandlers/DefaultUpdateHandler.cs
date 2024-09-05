using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    public class DefaultUpdateHandler : UpdateHandler
    {
        private IChatRepository _chats;
        public DefaultUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DefaultUpdateHandler> logger, IChatRepository chats ) : base(bot, scope, logger)
        {
            _chats = chats;
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
