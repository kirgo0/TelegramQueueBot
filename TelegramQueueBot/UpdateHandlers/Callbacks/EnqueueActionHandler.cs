﻿using Autofac;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Common.Action.Enqueue)]
    public class EnqueueActionHandler : UserNotifyingUpdateHandler
    {
        private QueueService _queueService;
        public EnqueueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<EnqueueActionHandler> logger, QueueService queueService) : base(bot, scope, logger)
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
            if(!user.ChatIds.ContainsKey(chat.Id))
            {
                user.ChatIds.Add(chat.Id, true);
                await _userRepository.UpdateAsync(user);
            }
            var action = GetAction(update);
            _log.LogInformation("User {id} from chat {chatId} requested {data}", user.TelegramId, chat.TelegramId, action);

            var actionData = action.Replace(Common.Action.Enqueue, string.Empty);
            if (!int.TryParse(actionData, out int pos))
            {
                _log.LogError("The position {pos} in the chat {id} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                if (chat.Mode is not Models.Enums.ChatMode.CallingUsers)
                {
                    await _queueService.EnqueueAsync(chat.CurrentQueueId, pos, user.TelegramId);
                }
                else
                {
                    var firstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
                    await _queueService.EnqueueAsync(chat.CurrentQueueId, pos, user.TelegramId);
                    var nextfirstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
                    await NotifyUsersIfOrderChanged(chat, firstTwoUsers, nextfirstTwoUsers);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while enqueing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }

        }

    }
}
