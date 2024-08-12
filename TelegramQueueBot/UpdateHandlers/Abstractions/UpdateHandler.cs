using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UpdateHandler
    {
        protected ITelegramBotClient _bot;

        protected UpdateHandler(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public abstract Task Handle(Update update);
    }
}
