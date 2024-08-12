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
    public class MessageUpdateHandler : UpdateHandler
    {
        public MessageUpdateHandler(ITelegramBotClient bot) : base(bot)
        {
        }

        public override async Task Handle(Update update)
        {
            Console.WriteLine("aasd");
        }
    }
}
