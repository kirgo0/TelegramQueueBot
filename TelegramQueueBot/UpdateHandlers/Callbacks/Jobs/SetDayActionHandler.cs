using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks.Jobs
{
    [HandleAction(Actions.SetDay)]
    public class SetDayActionHandler : UpdateHandler
    {
        public SetDayActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SetDayActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
        }

        public override Task Handle(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
