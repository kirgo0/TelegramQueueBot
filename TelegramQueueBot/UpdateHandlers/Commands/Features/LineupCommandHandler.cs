using Autofac;
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

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandleCommand(Command.LineUp)]
    public class LineupCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public LineupCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<LineupCommandHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _userRepository = userRepository;
            _queueService = queueService;
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
            var operationResult = await _queueService.RemoveBlankSpacesAsync(chat.CurrentQueueId);
            if (operationResult)
            {
                msg.AppendTextLine(TextResources.GetValue(TextKeys.RemovedAllBlankSpaces));
                await _bot.BuildAndSendAsync(msg);
            }
            else
            {
                msg.AppendText(TextResources.GetValue(TextKeys.NoBlankSpacesToRemove));
                await _bot.BuildAndSendAsync(msg);
            }
        }
    }
}
