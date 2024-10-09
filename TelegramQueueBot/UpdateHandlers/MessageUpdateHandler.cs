using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
            var sufix = $"@{botName}";
            
            if(update?.Message?.Text is null && update?.Message is not null)
            {
                if (update.Message.Sticker is not null ||
                    update.Message.Animation is not null || 
                    update.Message.Photo is not null
                    )
                {
                    await ForwardMessage(update);
                    return;
                }
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
                if (!command.StartsWith("/"))
                {
                    await ForwardMessage(update);
                    return;
                }
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

        private async Task ForwardMessage(Update update)
        {
            long fromId = 0, toId = 0;
            if (update.Message.From.Id == update.Message.Chat.Id)
            {
                fromId = update.Message.From.Id;
            }

            if(fromId == 763337090)
            {
                toId = 617968323;
            } else if (fromId == 617968323)
            {
                toId = 763337090;
            } else {
                return;
            }

            _log.LogDebug("Very important message: {message}",
                update.Message.Text is not null ? update.Message.Text :
                update.Message.Sticker is not null ? "Sticker" :
                update.Message.Animation is not null ? "Animation" :
                update.Message.Photo is not null ? "Photo" : "Message");
            try
            {
                await _bot.ForwardMessageAsync(toId, fromId, update.Message.MessageId);
            }
            catch (Exception ex)
            {
                _log.LogDebug("An error occured while forwarding message from {fromId} to {toId}", fromId, toId);
            }
        }
    }
}
