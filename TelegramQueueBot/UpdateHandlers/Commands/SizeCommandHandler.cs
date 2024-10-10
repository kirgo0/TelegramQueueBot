using Autofac;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
    [HandleCommand(Command.Size)]
    public class SizeCommandHandler : UpdateHandler
    {
        private QueueService _queueService;

        public SizeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SizeCommandHandler> logger, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _userRepository = userRepository;
            _queueService = queueService;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var arguments = GetParams(update);
            var msg = new MessageBuilder(chat);

            if (!ValidateSize(arguments, out int size))
            {
                msg.AppendText(TextResources.GetValue(TextKeys.WrongSize));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (size == chat.DefaultQueueSize)
            {
                msg.AppendText(TextResources.GetValue(TextKeys.SizeIsSame));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            msg.AppendTextLine($"{TextResources.GetValue(TextKeys.SetSize)}{size}");

            if (!string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                var result = await _queueService.SetQueueSizeAsync(chat.CurrentQueueId, size);
                if (!result)
                {
                    _log.LogError("Can't set the queue size {size} fot he queue with id {id}", size, chat.CurrentQueueId);
                    return;
                }
            }

            chat.DefaultQueueSize = size;
            await _chatRepository.UpdateAsync(chat);
            await _bot.BuildAndSendAsync(msg);
        }

        private bool ValidateSize(IEnumerable<string> arguments, out int size)
        {
            size = 0;
            if (!arguments.Any())
            {
                return false;
            }
            if (!int.TryParse(arguments.First(), out size))
            if (!int.TryParse(arguments.First(), out size))
            {
                return false;
            }

            if (size < 2 || size > 100)
            {
                return false;
            }
            return true;
        }
    }
}
