﻿using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class DequeueUpdateHandler : UpdateHandler
    {
        public DequeueUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DequeueUpdateHandler> logger) : base(bot, scope, logger)
        {
            CheckUserExists = true;
        }

        public override async Task Handle(Update update)
        {
            _log.LogInformation("User {id} requested {data}", update.CallbackQuery.From.Id, update.CallbackQuery.Data);
        }
    }
}