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

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class EnqueueActionHandler : UpdateHandler
    {
        public EnqueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<EnqueueActionHandler> logger) : base(bot, scope, logger)
        {
        }

        public override async Task Handle(Update update)
        {
            _log.LogInformation("User {id} from chat {chatId} requested {data}", update.CallbackQuery.From.Id, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data);
        }
    }
}
