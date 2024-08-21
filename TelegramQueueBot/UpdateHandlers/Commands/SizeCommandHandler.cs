using Autofac;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class SizeCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public SizeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SizeCommandHandler> logger, QueueService queueService, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            NeedsUser = true;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var arguments = GetParams(update);
            var msg = new MessageBuilder(chat);

            if(!ValidateSize(arguments, out int size))
            {
                // TODO: invalid size parameter message
                return;
            }

            if (size == chat.DefaultQueueSize)
            {
                // TODO: message if first argument is equal
                return;
            }

            try
            {
                var result = await _queueService.SetQueueSizeAsync(chat.CurrentQueueId, size, false);
                if (!result) return;

                var queue = await _queueService.GetQueueSnapshotAsync(chat.CurrentQueueId);
                if (queue is null)
                {
                    _log.LogError("An error occurred while retrieving a queue from the repository for chat {id}", chat.TelegramId);
                    return;
                }
                var names = await _userRepository.GetRangeByTelegramIdsAsync(queue.List);
                msg
                    .AppendTextLine($"Встановлено розмір - {size}")
                    .AppendText("Ну тіпа пасасав")
                    .AddDefaultQueueMarkup(names);

                await DeleteLastMessageAsync(chat);
                var response = await _bot.BuildAndSendAsync(msg);
                if (response is not null)
                {
                    chat.DefaultQueueSize = size;
                    chat.LastMessageId = response.MessageId;
                    await _chatRepository.UpdateAsync(chat);
                }

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occured while changing queue {id} size to {size}", chat.CurrentQueueId, size);
            }
        }

        private bool ValidateSize(IEnumerable<string> arguments, out int size)
        {
            size = 0;
            if (!arguments.Any())
            {
                // TODO: message if no arguments passed
                return false;
            }
            if (!int.TryParse(arguments.First(), out size))
            {
                // TODO: message if first argument is not a number
                return false;
            }

            if (size < 2)
            {
                // TODO: message if first argument is negative
                return false;
            }

            return true;
        }
    }
}
