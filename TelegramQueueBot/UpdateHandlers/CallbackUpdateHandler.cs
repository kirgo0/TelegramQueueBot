﻿using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    [HandlerMetadata(Metatags.HandleType, UpdateType.CallbackQuery)]
    public class CallbackUpdateHandler : UpdateHandler
    {
        public CallbackUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CallbackUpdateHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update,
                Metatags.HandleAction,
                (update, value, item) => update.CallbackQuery.Data.StartsWith(value.ToString()),
                "An error occurred while resolving the action handler for {data}",
                update?.CallbackQuery?.Data
                );
        }
    }
}
