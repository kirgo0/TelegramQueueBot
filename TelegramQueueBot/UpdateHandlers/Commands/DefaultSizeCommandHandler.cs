using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using Autofac;
using Microsoft.Extensions.Logging;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class DefaultSizeCommandHandler : UpdateHandler
    {
        public DefaultSizeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DefaultSizeCommandHandler> logger) : base(bot, scope, logger)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
