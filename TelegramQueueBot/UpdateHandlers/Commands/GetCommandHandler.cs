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

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandleCommand(Command.Get)]
    public class GetCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public GetCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<GetCommandHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
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
            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                msg.AppendText(TextResources.GetValue(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            await DeleteLastMessageAsync(chat);
            await _queueService.RerenderQueue(chat.CurrentQueueId);
        }
    }
}
