using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandlesCommand(Command.SkipFirst)]
    public class SkipFirstCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public SkipFirstCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SkipFirstCommandHandler> logger, QueueService queueSrevice, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
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
                msg.AppendTextLine(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (chat.Mode is not ChatMode.CallingUsers)
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.NeedToTurnOnCallingMode));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (await _queueService.IsQueueEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            await _queueService.DequeueFirstAsync(chat.CurrentQueueId, false);

            await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, async (queue) =>
            {
                if (queue.IsEmpty)
                {
                    msg.AppendTextLine(await _textRepository.GetValueAsync(TextKeys.QueueEndedCallingUsers));
                    chat.Mode = ChatMode.Open;
                }
                else
                {
                    msg
                        .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.QueueIsCallingUsers))
                        .AppendTextLine()
                        .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.FirstUserDequeued));
                }
                var names = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg
                    .AppendTextLine()
                    .AppendText(await _textRepository.GetValueAsync(TextKeys.CurrentQueue))
                    .AddDefaultQueueMarkup(names, chat.View);

                await DeleteLastMessageAsync(chat);
                await SendAndUpdateChatAsync(chat, msg, true);
            });
        }
    }
}
