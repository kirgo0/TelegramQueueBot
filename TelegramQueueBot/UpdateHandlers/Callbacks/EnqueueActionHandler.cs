using Autofac;
using Microsoft.Extensions.Logging;
using QueueCore;
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
        private IQueueService _queueService;
        public EnqueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<EnqueueActionHandler> logger, IQueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsUser = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            _log.LogInformation("User {id} from chat {chatId} requested {data}", update.CallbackQuery.From.Id, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data);
            var chat = await chatTask;
            //_queueService.EnqueueAsync(chat.CurrentQueueId, );
        }
    }
}
