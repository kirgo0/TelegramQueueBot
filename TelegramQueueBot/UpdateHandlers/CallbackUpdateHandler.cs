using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers
{
    public class CallbackUpdateHandler : UpdateHandler
    {
        public CallbackUpdateHandler(ITelegramBotClient bot) : base(bot)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
