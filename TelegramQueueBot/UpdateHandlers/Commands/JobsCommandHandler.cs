using Autofac;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Commands
{
    [HandlesCommand(Command.Jobs)]
    public class JobsCommandHandler : UpdateHandler
    {
        private IChatJobRepository _jobRepository;
        public JobsCommandHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<JobsCommandHandler> logger,  IChatJobRepository jobRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsChat = true;
            _jobRepository = jobRepository;
        }

        public override async Task Handle(Update update)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var recurringJobs = connection.GetRecurringJobs();
                //foreach (var recurringJob in recurringJobs)
                //{
                //    RecurringJob.RemoveIfExists(recurringJob.Id);
                //}
                _log.LogDebug("Jobs available: {count}", recurringJobs.Count);
            }
            var chat = await chatTask;
            var msg = new MessageBuilder(chat);

            var jobs = await _jobRepository.GetAllByChatIdAsync(chat.TelegramId);

            if (jobs.Count == 0)
            {
                msg.AppendText(TextResources.GetValue(TextKeys.JobsListIsEmpty));
            }
            else
            {
                msg.AppendText(TextResources.GetValue(TextKeys.JobsList));
                foreach (var job in jobs)
                {
                    msg.AddButtonNextRow(job.JobName, $"{Actions.JobMenu}{job.Id}");
                }
            }
            if (update.CallbackQuery is not null)
            {
                await _bot.BuildAndEditAsync(msg);
            }
            else
            {
                await DeleteLastMessageAsync(chat);
                await SendAndUpdateChatAsync(chat, msg);
            }
        }
    }
}
