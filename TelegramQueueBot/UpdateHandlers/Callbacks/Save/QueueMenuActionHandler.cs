﻿using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Save
{
    [HandleAction(Common.Action.QueueMenu)]
    public class QueueMenuActionHandler : UpdateHandler
    {
        QueueService _queueService;
        public QueueMenuActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<QueueMenuActionHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            var queueId = GetAction(update).Replace(Common.Action.QueueMenu, string.Empty);
            try
            {
                var queue = await _queueService.GetByIdAsync(queueId);
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);

                msg
                    .AppendText($"{TextResources.GetValue(TextKeys.QueueMenu)}{queue.Name}")
                    .AddButton(TextResources.GetValue(TextKeys.BackBtn), $"{Common.Action.QueueList}")
                    .AddButton(TextResources.GetValue(TextKeys.LoadQueueBtn), $"{Common.Action.Load}{queueId}")
                    .AddButtonNextRow(TextResources.GetValue(TextKeys.DeleteQueueBtn), $"{Common.Action.Delete}{queueId}")
                    .AddSavedQueueMarkup(users);

                await _bot.BuildAndEditAsync(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error ocured when creating a menu for a queue with id {queueId}", queueId);
            }
        }
    }
}
