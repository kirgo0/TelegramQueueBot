using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class DefaultSizeCommandHandler : UpdateHandler
    {
        public DefaultSizeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DefaultSizeCommandHandler> logger) : base(bot, scope, logger)
        {
            GroupsOnly = true;
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
