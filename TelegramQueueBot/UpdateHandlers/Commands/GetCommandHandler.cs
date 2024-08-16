using Autofac;
using Microsoft.Extensions.Logging;
using QueueCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class GetCommandHandler : UpdateHandler
    {
        private IQueueService _queueService;
        public GetCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<GetCommandHandler> logger, IQueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var queue = await _queueService.GetQueueSnapshotAsync(chat.CurrentQueueId);
            if (queue is null)
            {
                _log.LogError("An error occurred while retrieving a queue from the repository for chat {id}", chat.TelegramId);
                return;
            }
            var dict = await _userRepository.GetUsernamesAsync(queue.List);
            var names = QueueHandler.GetQueueNames(queue.List, dict);

            var msg = new MessageBuilder(chat)
                .AppendText("Ну тіпа пасасав")
                .AddDefaultQueueMarkup(names);

            await _bot.BuildAndSendAsync(msg);
        }
    }
}
