using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Save
{
    [HandlesCommand(Command.SavedList)]
    public class SavedListCommandHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public SavedListCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SavedListCommandHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if (!chat.SavedQueuesIds.Any())
            {
                // handling redirection from QueueListActionHandler
                if (update.CallbackQuery is not null)
                {
                    await DeleteLastMessageAsync(chat);
                    chat.LastMessageId = 0;
                    await _chatRepository.UpdateAsync(chat);
                }
                msg.AppendText(TextResources.GetValue(TextKeys.NoSavedQueues));
                await _bot.BuildAndSendAsync(msg);
                return;
            }
            var queues = await _queueService.GetByIdsAsync(chat.SavedQueuesIds);
            if (!queues.Any())
            {
                _log.LogError("An error ocured while retrieving queues for chat {chatId}", chat.Id);
                return;
            }

            msg.AppendText(TextResources.GetValue(TextKeys.SavedQueuesList));

            foreach (var queue in queues)
            {
                msg.AddButtonNextRow(queue.Name, $"{Actions.QueueMenu}{queue.Id}");
            }

            // handling redirection from QueueListActionHandler
            if (update.CallbackQuery is not null)
            {
                await _bot.BuildAndEditAsync(msg);
            }
            else
            {
                await DeleteLastMessageAsync(chat);
                await SendAndUpdateChatAsync(chat, msg);
            }
        }
    }
}
