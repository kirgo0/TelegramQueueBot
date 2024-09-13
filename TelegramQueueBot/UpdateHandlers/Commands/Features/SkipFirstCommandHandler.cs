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
    public class SkipFirstCommandHandler : UserNotifyingUpdateHandler
    {
        private QueueService _queueService;
        public SkipFirstCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SkipFirstCommandHandler> logger, QueueService queueSrevice) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            NeedsUser = true;
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

            if (await _queueService.IsQueueEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(TextResources.GetValue(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            var firstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
            await _queueService.DequeueFirstAsync(chat.CurrentQueueId, false);
            var nextfirstTwoUsers = await _queueService.GetRangeAsync(chat.CurrentQueueId, 2);
            await NotifyUsersIfOrderChanged(chat, firstTwoUsers, nextfirstTwoUsers);

            await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, async (queue) =>
            {
                if (queue.IsEmpty)
                {
                    msg.AppendTextLine(TextResources.GetValue(TextKeys.QueueEndedCallingUsers));
                    chat.Mode = ChatMode.Open;
                }
                else
                {
                    msg
                        .AppendTextLine(TextResources.GetValue(TextKeys.QueueIsCallingUsers))
                        .AppendTextLine()
                        .AppendTextLine(TextResources.GetValue(TextKeys.FirstUserDequeued));
                }
                var names = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg.AddDefaultQueueMarkup(names, chat.View);
            });
            msg
                .AppendTextLine()
                .AppendText(TextResources.GetValue(TextKeys.CurrentQueue));

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg, true);
        }
    }
}
