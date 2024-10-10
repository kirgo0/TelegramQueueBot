using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands.Features
{
    [HandleCommand(Command.View)]
    public class ViewCommandHandler : UpdateHandler
    {
        private readonly QueueService _queueService;
        public ViewCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ViewCommandHandler> logger, QueueService queueService) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _queueService = queueService;
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
            await _queueService.RerenderQueue(chat.CurrentQueueId);
        }
    }
}
