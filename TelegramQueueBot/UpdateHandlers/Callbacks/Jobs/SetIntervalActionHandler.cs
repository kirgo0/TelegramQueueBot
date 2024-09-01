﻿using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.SetInterval)]
    public class SetIntervalActionHandler : UpdateHandler
    {
        public SetIntervalActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetIntervalActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}

