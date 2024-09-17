using Autofac;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        public MessageUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<MessageUpdateHandler> logger, IConfiguration configuration) : base(bot, scope, logger)
        {
            _configuration = configuration;
        }

        public override async Task Handle(Update update)
        {
            var sufix = $"@{_configuration.GetSection("TelegramBotOptions")["BotName"]}";
            
            if(update?.Message?.Text is null && update?.Message is not null)
            {
                if (update.Message.Sticker is not null) return;
                if (update.Message.Animation is not null) return;
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
                var command = update.Message.Text;
                if (!command.StartsWith("/")) return;
                if (command.Contains("@") && !command.Contains(sufix)) return;
                await RedirectHandle(
                    update,
                    Metatags.HandleCommand,
                    (value) =>
                    command.Split(' ')[0]
                        .Replace(sufix, "")
                        .Equals(value.ToString()),
                    "An error occurred while resolving the command handler for {text}",
                    update.Message.Text
                );
            }
        }
    }
}
