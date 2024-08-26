﻿using Autofac;
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
    public class GetCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public GetCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<GetCommandHandler> logger, QueueService queueService, IUserRepository userRepository, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
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
                msg.AppendText(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg); 
                return; 
            }
            await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, async (queue) =>
            {
                if(queue.Count == 0)
                {
                    msg.AddEmptyQueueMarkup(queue.Size, chat.View);
                    return;
                }
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg.AddDefaultQueueMarkup(users, chat.View);
            });

            await msg.AppendModeTitle(chat, _textRepository);
            msg.AppendText(await _textRepository.GetValueAsync(TextKeys.CurrentQueue));

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg);
        }
    }
}