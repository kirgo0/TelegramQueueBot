using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class LoadActionHandler : UpdateHandler
    {
        public LoadActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<LoadActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
