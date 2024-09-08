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
    [HandlerMetadata(Metatags.HandleType, UpdateType.CallbackQuery)]
    public class CallbackUpdateHandler : UpdateHandler
    {
        public CallbackUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CallbackUpdateHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update,
                Metatags.HandleAction,
                (value) => update.CallbackQuery.Data.StartsWith(value.ToString()),
                "An error occurred while resolving the action handler for {data}",
                update?.CallbackQuery?.Data
                );
        }
    }
}
