using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class SkipFirstCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public SkipFirstCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SkipFirstCommandHandler> logger, QueueService queueSrevice) : base(bot, scope, logger)
        {
            NeedsChat = true;
            NeedsUser = true;
            _queueService = queueSrevice;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            if(chat.Mode is not ChatMode.CallingUsers)
            {
                // TODO: if calling users mode turned off message
                return;
            }

            var result = await _queueService.DequeueFirstAsync(chat.CurrentQueueId, false);
            if (!result)
            {
                // TODO: message if queue is empty
                return;
            }
            var deleteTask = DeleteLastMessageAsync(chat);
            var queue = await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, async (queue) =>
            {
                var names = await _userRepository.GetRangeByTelegramIdsAsync(queue.List);
                msg
                    .AppendTextLine("Пропущено першого корситувача")
                    .AppendText("Ну тіпа пасасав")
                    .AddDefaultQueueMarkup(names);

                var response = await _bot.BuildAndSendAsync(msg);
                if (response is not null)
                {
                    await deleteTask;
                    chat.LastMessageId = response.MessageId;
                    await _chatRepository.UpdateAsync(chat);
                }
            });
        }
    }
}
