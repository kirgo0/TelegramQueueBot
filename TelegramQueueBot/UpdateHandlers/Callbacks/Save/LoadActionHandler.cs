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
    [HandleAction(Common.Action.Load)]
    public class LoadActionHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public LoadActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<LoadActionHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
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
            var queueId = GetAction(update).Replace(Common.Action.Load, string.Empty);

            chat.CurrentQueueId = queueId;
            chat.Mode = Models.Enums.ChatMode.Open;
            await _queueService.DoThreadSafeWorkOnQueueAsync(queueId, async (queue) =>
            {
                chat.DefaultQueueSize = queue.Size;
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg
                    .AppendText($"{TextResources.GetValue(TextKeys.CurrentQueue)} - {queue.Name}")
                    .AddDefaultQueueMarkup(users, chat.View);
            });

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg, true);

        }
    }
}
