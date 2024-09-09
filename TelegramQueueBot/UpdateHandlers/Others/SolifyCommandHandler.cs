using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Others
{
    [HandleCommand("/solify")]
    public class SolifyCommandHandler : UpdateHandler
    {
        public SolifyCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SolifyCommandHandler> logger) : base(bot, scope, logger)
        {
            NeedsUser = true;
        }

        public override async Task Handle(Update update)
        {
            var user = await userTask;
            await _bot.SendAnimationAsync(
                user.TelegramId,
                animation: new InputFileId("CgACAgQAAxkBAAIOOGbeByXas2w5kbeOslNQkpFECVEJAAIxAwACBXAFU8OvjQJvikg0NgQ"),
                caption: "Як до цього взагалі можна було додуматись? \n\n<b><s>/solify</s> /notify</b>",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                duration: 6
                );
        }
    }
}
