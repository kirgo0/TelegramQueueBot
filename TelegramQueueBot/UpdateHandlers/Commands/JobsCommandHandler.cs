using Autofac;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Jobs;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    public class JobsCommandHandler : UpdateHandler
    {
        public JobsCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobsCommandHandler> logger, ITextRepository textRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            msg.AppendText(await _textRepository.GetValueAsync(TextKeys.JobsList));

        }
    }
}
