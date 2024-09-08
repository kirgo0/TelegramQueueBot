using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Others
{
    [HandlesCommand("/test")]
    public class TestCommandHandler : UpdateHandler
    {
        public TestCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<TestCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
        }
    }
}
