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
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    public class DeleteActionHandler : UpdateHandler
    {
        public DeleteActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DeleteActionHandler> logger, ITextRepository textRepository, QueueService queueService) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            var queueId = GetAction(update).Replace(Actions.Delete, string.Empty);
            msg
                .AppendText($"{await _textRepository.GetValueAsync(TextKeys.ConfirmDeletion)}")
                .AddButton(await _textRepository.GetValueAsync(TextKeys.BackBtn), $"{Actions.QueueMenu}{queueId}")
                .AddButton(await _textRepository.GetValueAsync(TextKeys.ConfirmDeletionBtn), $"{Actions.ConfirmDeletion}{queueId}" );

            await _bot.BuildAndEditAsync(msg);
        }
    }
}
