﻿using Autofac;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.UpdateHandlers;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.Modules
{
    public class HandlersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageUpdateHandler>()
                .As<UpdateHandler>()
                .WithMetadata("type", UpdateType.Message);
            builder.RegisterType<CallbackUpdateHandler>()
                .As<UpdateHandler>()
                .WithMetadata("type", UpdateType.CallbackQuery);
        }
    }
}
