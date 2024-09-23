using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandleCommand(Command.Reload)]
    public class ReloadCommandHandler : UpdateHandler
    {
        public ReloadCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ReloadCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
        }

        public override async Task Handle(Update update)
        {
            var user = await userTask;
            if(user.TelegramId != 617968323)
            {
                return;
            }

            _log.LogInformation("User {telegramId} has requested a text reload", user.TelegramId);
            await TextResources.ReloadFromPreviousContext(typeof(TextKeys));
        }
    }
}
