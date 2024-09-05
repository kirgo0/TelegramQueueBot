using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandlesCommand(Command.View)]
    public class ViewCommandHandler : UpdateHandler
    {
        public ViewCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ViewCommandHandler> logger ) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            chat.View = chat.View.Next();
            var textForType = string.Empty;
            switch (chat.View)
            {
                case ViewType.Table: textForType = TextResources.GetValue(TextKeys.ChatViewTable); break;
                case ViewType.Column: textForType = TextResources.GetValue(TextKeys.ChatViewColumn); break;
                case ViewType.Auto: textForType = TextResources.GetValue(TextKeys.ChatViewAuto); break;
            }
            msg.AppendText($"{TextResources.GetValue(TextKeys.ChangedChatView)}{textForType}");
            await _chatRepository.UpdateAsync(chat);
            await _bot.BuildAndSendAsync(msg);
        }
    }
}
