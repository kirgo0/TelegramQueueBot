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
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class ViewCommandHandler : UpdateHandler
    {
        public ViewCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ViewCommandHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            chat.View = chat.View.Next();
            var titleTask = _textRepository.GetValueAsync(TextKeys.ChangedChatView);
            var textForType = "";
            switch (chat.View)
            {
                case ViewType.Table: textForType = await _textRepository.GetValueAsync(TextKeys.ChatViewTable); break;
                case ViewType.Column: textForType = await _textRepository.GetValueAsync(TextKeys.ChatViewColumn); break;
                case ViewType.Auto: textForType = await _textRepository.GetValueAsync(TextKeys.ChatViewAuto); break;
            }
            msg.AppendText($"{await titleTask}{textForType}");
            await _chatRepository.UpdateAsync(chat);
            await _bot.BuildAndSendAsync(msg);
        }
    }
}
