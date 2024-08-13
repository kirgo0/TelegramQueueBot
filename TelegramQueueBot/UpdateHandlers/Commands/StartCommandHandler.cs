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
        private IUserRepository _users;
        public StartCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<StartCommandHandler> logger, IUserRepository users) : base(bot, scope, logger)
        {
            _users = users;
        }

        public override async Task Handle(Update update)
        {
            User user = await _users.GetByTelegramIdAsync(update.Message.From.Id);
            if(user is null)
            {
                user = new(update.Message.From.Id, update.Message.From.Username);
                var res = await _users.CreateAsync(user);
            }

        }
    }
}
