using Autofac;
using Microsoft.Extensions.Logging;
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
            NeedsUser = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            if (chat is null)
            {
                var user = await userTask;
                var msg = new MessageBuilder().SetChatId(user.TelegramId);
                AddHelpmenu(msg);
                await _bot.BuildAndSendAsync(msg);

            }
            else
            {
                var msg = new MessageBuilder(chat);
                AddHelpmenu(msg);
                await DeleteLastMessageAsync(chat);
                await SendAndUpdateChatAsync(chat, msg);
            }
        }

        public MessageBuilder AddHelpmenu(MessageBuilder msg)
        {
            return msg
                .AppendText(TextResources.GetValue(TextKeys.DefaultHelp))
                .AddButton(TextResources.GetValue(TextKeys.FeaturesHelpBtn), $"{Common.Action.Help}{TextKeys.FeaturesHelpBtn}")
                .AddButton(TextResources.GetValue(TextKeys.CallingModeHelpBtn), $"{Common.Action.Help}{TextKeys.CallingModeHelpBtn}");
        }
    }
}
