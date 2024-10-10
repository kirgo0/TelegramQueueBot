using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandleCommand(Command.SkipFirst)]
    public class SkipFirstCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public SkipFirstCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SkipFirstCommandHandler> logger, QueueService queueSrevice) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueSrevice;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendTextLine(TextResources.GetValue(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (chat.Mode is not ChatMode.CallingUsers)
            {
                msg.AppendText(TextResources.GetValue(TextKeys.NeedToTurnOnCallingMode));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            var queueCount = await _queueService.GetQueueCountAsync(chat.CurrentQueueId);
            if (queueCount == 0)
            {
                msg.AppendText(TextResources.GetValue(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }
            
            if(queueCount != 1)
            {
                await _queueService.DequeueFirstAsync(chat.CurrentQueueId);
            } else
            {
                if(await _queueService.DequeueFirstAsync(chat.CurrentQueueId))
                {
                    chat.Mode = ChatMode.Open;
                    msg.AppendTextLine(TextResources.GetValue(TextKeys.QueueEndedCallingUsers));
                    await _chatRepository.UpdateAsync(chat);
                    await _bot.BuildAndSendAsync(msg);
                }
            }
        }
    }
}
