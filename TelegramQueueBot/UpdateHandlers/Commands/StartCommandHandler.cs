using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandleCommand(Command.Start)]
    public class StartCommandHandler : UpdateHandler
    {
        public StartCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<StartCommandHandler> logger, IUserRepository users, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            NeedsChat = true;
            NeedsUser = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var user = await userTask;
            if (chat is null)
            {
                user.IsAuthorized = true;
                await _userRepository.UpdateAsync(user);
                var msg = new MessageBuilder()
                    .SetChatId(user.TelegramId)
                    .AppendText(await _textRepository.GetValueAsync(TextKeys.Start));
                await _bot.BuildAndSendAsync(msg);
            }

            if (chat is not null)
            {
                // TODO: remove deletion on start
                await DeleteLastMessageAsync(chat);
                var msg = new MessageBuilder(chat)
                    .AppendText(await _textRepository.GetValueAsync(TextKeys.Start));
                await _bot.BuildAndSendAsync(msg);
            }

        }
    }
}
