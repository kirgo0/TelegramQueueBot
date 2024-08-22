﻿using Autofac;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
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
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.WrongSize));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (size == chat.DefaultQueueSize)
            {
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.SizeIsSame));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            try
            {
                msg
                    .AppendTextLine($"{await _textRepository.GetValueAsync(TextKeys.SetSize)}{size}")
                    .AppendTextLine();

                if (!string.IsNullOrEmpty(chat.CurrentQueueId))
                {
                    var result = await _queueService.SetQueueSizeAsync(chat.CurrentQueueId, size, false);
                    if (!result) return;
                    // TODO: review logic

                    var queue = await _queueService.GetQueueSnapshotAsync(chat.CurrentQueueId);
                    if (queue is null)
                    {
                        _log.LogError("An error occurred while retrieving a queue from the repository for chat {id}", chat.TelegramId);
                        return;
                    }
                    var names = await _userRepository.GetRangeByTelegramIdsAsync(queue.List);
                    msg
                        .AppendText(await _textRepository.GetValueAsync(TextKeys.CurrentQueue))
                        .AddDefaultQueueMarkup(names);
                    await DeleteLastMessageAsync(chat);
                }

                var response = await _bot.BuildAndSendAsync(msg);
                chat.DefaultQueueSize = size;
                if (response is not null && !string.IsNullOrEmpty(chat.CurrentQueueId))
                {
                    chat.LastMessageId = response.MessageId;
                }
                await _chatRepository.UpdateAsync(chat);

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
                return false;
            }
            if (!int.TryParse(arguments.First(), out size))
            {
                return false;
            }

            if (size < 2)
            {
                return false;
            }

            return true;
        }
    }
}
