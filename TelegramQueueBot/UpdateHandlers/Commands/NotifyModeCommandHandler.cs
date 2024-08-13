using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class NotifyModeCommandHandler : UpdateHandler
    {
        public NotifyModeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<NotifyModeCommandHandler> logger) : base(bot, scope, logger)
        {
            GroupsOnly = true;
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
