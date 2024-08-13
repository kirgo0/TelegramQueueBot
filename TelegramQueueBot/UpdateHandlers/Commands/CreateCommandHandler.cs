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
    public class CreateCommandHandler : UpdateHandler
    {
        public CreateCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CreateCommandHandler> logger) : base(bot, scope, logger)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
