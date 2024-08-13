using Autofac;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramQueueBot.DataAccess.Abstraction;
using TelegramQueueBot.Models;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using User = TelegramQueueBot.Models.User;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class StartCommandHandler : UpdateHandler
    {
        public StartCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<StartCommandHandler> logger, IUserRepository users) : base(bot, scope, logger)
        {
            CheckChatExists = true;
        }

        public override async Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
