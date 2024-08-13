using Autofac;
using Microsoft.Extensions.Logging;
using QueueCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class CreateCommandHandler : UpdateHandler
    {
        public CreateCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<CreateCommandHandler> logger, IQueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            CheckChatExists = true;
        }

        public override async Task Handle(Update update)
        {
            //var msg = new MessageBuilder()
            //    .SetChatId(update.Message.Chat.Id)
            //    .AppendText("Aboba")
            //    .AddDefaultQueueMarkup();

            //await _bot.BuildAndSendAsync(msg);
        }
    }
}
