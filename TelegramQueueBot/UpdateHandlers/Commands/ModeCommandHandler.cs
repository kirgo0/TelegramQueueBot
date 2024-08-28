using Autofac;
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
    [HandleCommand(Command.Mode)]
    public class ModeCommandHandler : UpdateHandler
    {
        private QueueService _queueService;
        public ModeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ModeCommandHandler> logger, ITextRepository textRepository, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger, textRepository)
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
                msg.AppendTextLine(await _textRepository.GetValueAsync(TextKeys.NoCreatedQueue));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            if (await _queueService.IsQueueEmpty(chat.CurrentQueueId))
            {
                msg.AppendTextLine(await _textRepository.GetValueAsync(TextKeys.QueueIsEmpty));
                await _bot.BuildAndSendAsync(msg);
                return;
            }

            chat.Mode = chat.Mode.Next();

            await msg.AppendModeTitle(chat, _textRepository);
            msg.AppendText(await _textRepository.GetValueAsync(TextKeys.CurrentQueue));

            await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, async (queue) =>
            {
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg.AddDefaultQueueMarkup(users, chat.View);
            });

            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg, true);
        }
    }
}
