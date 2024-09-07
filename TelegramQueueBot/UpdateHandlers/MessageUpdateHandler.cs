using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    [HandlerMetadata(Metatags.HandleType, UpdateType.Message)]
    public class MessageUpdateHandler : UpdateHandler
    {
        public MessageUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<MessageUpdateHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            var sufix = Command.BotSuffix;
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) =>
                update.Message.Text.Split(' ')[0]
                    .Replace(sufix, "")
                    .Equals(value.ToString()),
                "An error occurred while resolving the command handler for {text}",
                update.Message.Text
                );
        }
    }
}
