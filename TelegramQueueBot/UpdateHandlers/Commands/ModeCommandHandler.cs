using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class ModeCommandHandler : UpdateHandler
    {
        public ModeCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<ModeCommandHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            switch(chat.Mode)
            {
                case ChatMode.Open: chat.Mode = ChatMode.CallingUsers; break;
                case ChatMode.CallingUsers: chat.Mode = ChatMode.Open; break;
            }
            var result = await _chatRepository.UpdateAsync(chat);
            if(result)
            {
                var msg = new MessageBuilder(chat);
                if (chat.Mode == ChatMode.CallingUsers)
                {
                    msg.AppendText("Прохід по черзі - ✅");
                } else if(chat.Mode == ChatMode.Open)
                {
                    msg.AppendText("Прохід по черзі - ❌");
                }
                await _bot.BuildAndSendAsync(msg);
            }
        }
    }
}
