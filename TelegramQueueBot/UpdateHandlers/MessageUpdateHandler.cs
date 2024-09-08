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
            if(update?.Message?.Text is null && update?.Message is not null)
            {
                await RedirectHandle(
                    update,
                    Metatags.HandleMessageEvent,
                    (value) =>
                    {
                        var property = update.Message.GetType().GetProperty(value.ToString());
                        if(property is not null)
                            return property.GetValue(update.Message) is not null;
                        return false;
                    },
                    "An error occured while resolving message event handler for {update}",
                    update
                );
            } else
            {
                await RedirectHandle(
                    update,
                    Metatags.HandleCommand,
                    (value) =>
                    update.Message.Text.Split(' ')[0]
                        .Replace(sufix, "")
                        .Equals(value.ToString()),
                    "An error occurred while resolving the command handler for {text}",
                    update.Message.Text
                );
            }
        }
    }
}
