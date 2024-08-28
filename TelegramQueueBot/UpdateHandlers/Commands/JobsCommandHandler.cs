using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandleCommand(Command.Jobs)]
    public class JobsCommandHandler : UpdateHandler
    {
        private IChatJobRepository _jobRepository;
        public JobsCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobsCommandHandler> logger, ITextRepository textRepository, IChatJobRepository jobRepository) : base(bot, scope, logger, textRepository)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobRepository = jobRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            var list = await _jobRepository.GetAllByChatIdAsync(chat.TelegramId);
            msg.AppendText(await _textRepository.GetValueAsync(TextKeys.JobsList));

        }
    }
}
