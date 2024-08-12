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
    public class StartCommandHandler : UpdateHandler
    {
        public StartCommandHandler(ITelegramBotClient bot) : base(bot)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
