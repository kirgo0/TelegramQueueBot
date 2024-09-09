using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandleCommand(Command.Help)]
    public class HelpCommandHandler : UpdateHandler
    {
        public HelpCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<HelpCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            msg
                .AppendText(TextResources.GetValue(TextKeys.DefaultHelp))
                .AddButton(TextResources.GetValue(TextKeys.FeaturesHelpBtn), $"{Common.Action.Help}{TextKeys.FeaturesHelpBtn}")
                .AddButton(TextResources.GetValue(TextKeys.CallingModeHelpBtn), $"{Common.Action.Help}{TextKeys.CallingModeHelpBtn}");
            await DeleteLastMessageAsync(chat);
            await SendAndUpdateChatAsync(chat, msg);
        }
    }
}
