using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class QueueListActionHandler : UpdateHandler
    {
        public QueueListActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<QueueListActionHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            await RedirectHandle(
                update,
                Metatags.HandleCommand,
                (update, value, item) => value == Common.Commands.SavedList,
                "An error ocured while redirecting from {from} to {to}",
                Actions.QueueList, Common.Commands.SavedList
                );

        }
    }
}
