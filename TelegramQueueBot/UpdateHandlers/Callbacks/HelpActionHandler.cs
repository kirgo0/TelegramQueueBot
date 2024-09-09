using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Common.Action.Help)]
    public class HelpActionHandler : UpdateHandler
    {
        public HelpActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<HelpActionHandler> logger) : base(bot, scope, logger)
        {
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var action = GetAction(update);
            var data = action.Replace(Common.Action.Help, string.Empty);
            var helpTextKeys = new Dictionary<string, string>();
            helpTextKeys.Add(TextKeys.DefaultHelpBtn, TextKeys.DefaultHelp);
            helpTextKeys.Add(TextKeys.FeaturesHelpBtn, TextKeys.Features_Help);
            helpTextKeys.Add(TextKeys.CallingModeHelpBtn, TextKeys.CallingModeHelp);
            if(!helpTextKeys.TryGetValue(data, out string dataValue))
            {
                _log.LogWarning("The {data} help key is not registered in helpTextKeys of {name} handler", data, GetType().Name);
                return;
            }
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            msg.AppendText(TextResources.GetValue(dataValue));
            helpTextKeys.Remove(data);

            foreach(var btnKey in helpTextKeys.Keys)
            {
                msg.AddButton(TextResources.GetValue(btnKey), $"{Common.Action.Help}{btnKey}");
            }

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
