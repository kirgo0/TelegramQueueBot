using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class StartCommandHandler : UpdateHandler
    {
        public StartCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<StartCommandHandler> logger, IUserRepository users) : base(bot, scope, logger)
        {
            CheckChatExists = true;
        }

        public override async Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
