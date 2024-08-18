using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    public class MessageUpdateHandler : UpdateHandler
    {
        public MessageUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<MessageUpdateHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            var sufix = Common.Commands.BotSuffix;
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) => value.ToString().StartsWith(update?.Message?.Text.Replace(sufix, "")),
                "An error occurred while resolving the command handler for {text}",
                update.Message.Text
                );
        }
    }
}
