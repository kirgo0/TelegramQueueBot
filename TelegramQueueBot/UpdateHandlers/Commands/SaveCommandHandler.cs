using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class SaveCommandHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public SaveCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SaveCommandHandler> logger, ITextRepository textRepository, QueueService queueService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var name = GetParams(update).FirstOrDefault();
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (await _queueService.IsQueueEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = DateTime.Now.ToString();
            }

            var operationResult = await _queueService.SetQueueNameAsync(chat.CurrentQueueId, name);
            if (!operationResult)
            {
                msg.AppendText($"{await _textRepository.GetValueAsync(TextKeys.QueueIsAlreadySaved)}{name}");
            }
            else if (!chat.SavedQueuesIds.Contains(chat.CurrentQueueId))
            {
                chat.SavedQueuesIds.Add(chat.CurrentQueueId);
                msg.AppendText($"{await _textRepository.GetValueAsync(TextKeys.QueueSavedAs)}{name}");
                await _chatRepository.UpdateAsync(chat);
            }
            else
            {
                msg.AppendText($"{await _textRepository.GetValueAsync(TextKeys.ChangedSavedQueueName)}{name}");
            }
            await _bot.BuildAndSendAsync(msg);
        }
    }
}
