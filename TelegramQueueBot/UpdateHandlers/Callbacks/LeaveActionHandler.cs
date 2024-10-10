using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using Action = TelegramQueueBot.Common.Action;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Action.Leave)]
    public class LeaveActionHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public LeaveActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<LeaveActionHandler> logger, QueueService queueService, IChatRepository chatRepository) : base(bot, scope, logger)
        {
            NeedsUser = true;
            _queueService = queueService;
            _chatRepository = chatRepository;
        }

        public override async Task Handle(Update update)
        {
            var user = await userTask;
            var userMsg = new MessageBuilder().SetChatId(user.TelegramId);
            var actionData = GetAction(update).Replace(Action.Leave, string.Empty);
            if(!long.TryParse(actionData, out long chatId))
            {
                _log.LogError("Action data can not be parsed {data} for action {action}", actionData, Action.Leave);
                return;
            }
            var chat = await _chatRepository.GetByTelegramIdAsync(chatId);
            if(chat?.CurrentQueueId is null || chat.Mode is not Models.Enums.ChatMode.CallingUsers)
            {
                userMsg.AppendText(TextResources.GetValue(TextKeys.LeaveActionOutdated));
                await _bot.DeleteMessageAsync(user.TelegramId, update.CallbackQuery.Message.MessageId);
                await _bot.BuildAndSendAsync(userMsg);
                return;
            }

            var queueCount = await _queueService.GetQueueCountAsync(chat.CurrentQueueId);
            var msg = new MessageBuilder(chat);
            if (queueCount == 1)
            {
                var result = await _queueService.DequeueIfFirstAsync(chat.CurrentQueueId, user.TelegramId);

                if (!result) return;

                chat.Mode = Models.Enums.ChatMode.Open;

                msg.AppendTextLine(TextResources.GetValue(TextKeys.QueueEndedCallingUsers));
                await _bot.BuildAndSendAsync(msg);
            } else 
            {
                await _queueService.DequeueIfFirstAsync(chat.CurrentQueueId, user.TelegramId);
            }

        }
    }
}
