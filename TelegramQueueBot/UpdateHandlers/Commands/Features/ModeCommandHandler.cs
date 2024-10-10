using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandleCommand(Command.Mode)]
    public class ModeCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public ModeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ModeCommandHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
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
                msg.AppendTextLine(TextResources.GetValue(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (await _queueService.IsQueueEmpty(chat.CurrentQueueId))
            {
                msg.AppendTextLine(TextResources.GetValue(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            chat.Mode = chat.Mode.Next();

            await _chatRepository.UpdateAsync(chat);
            await DeleteLastMessageAsync(chat);
            await _queueService.RerenderQueue(chat.CurrentQueueId);
        }
    }
}
