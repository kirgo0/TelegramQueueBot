using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    public class ConfirmJobDeletionActionHandler : UpdateHandler
    {
        public ConfirmJobDeletionActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ConfirmJobDeletionActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}

