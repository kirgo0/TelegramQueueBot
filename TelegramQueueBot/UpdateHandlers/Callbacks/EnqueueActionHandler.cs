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
using TelegramQueueBot.Common;
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
            var chat = await chatTask;
            var user = await userTask;
            var action = GetAction(update);
            _log.LogInformation("User {id} from chat {chatId} requested {data}", user.TelegramId, chat.TelegramId, action);

            var actionData = action.Replace(Actions.Enqueue, string.Empty);
            if (!int.TryParse(actionData, out int pos))
            {
                _log.LogError("The position {pos} in the chat {id} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                await _queueService.EnqueueAsync(chat.CurrentQueueId, pos, user.TelegramId);
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while enqueing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }
        }
    }
}
