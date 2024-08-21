using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class SaveCommandHandler : UpdateHandler
    {
        public SaveCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<SaveCommandHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var name = GetParams(update).First();
            if (string.IsNullOrEmpty(name))
            {
                // TODO: no queue name specified message
                return;
            }
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);
            if (string.IsNullOrEmpty(chat.CurrentQueueId))
            {
                // TODO: no qeueue message
                return;
            }

            if (!chat.SavedQueuesIds.Contains(chat.CurrentQueueId))
            {
                chat.SavedQueuesIds.Add(chat.CurrentQueueId);
                msg.AppendText($"Чергу {name}, збережено");
                await DeleteLastMessageAsync(chat);
                var result = await _bot.BuildAndSendAsync(msg);
                chat.LastMessageId = result.MessageId;
                await _chatRepository.UpdateAsync(chat);
            }
            else
            {
                msg.AppendText($"Черга вже збережена");
                await _bot.BuildAndSendAsync(msg);
            }
        }
    }
}
