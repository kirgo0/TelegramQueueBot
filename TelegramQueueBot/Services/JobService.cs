using Cronos;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services
{
    public class JobService
    {
        private readonly IChatJobRepository _chatJobRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IChatRepository _chatRepository;
        private readonly ITextRepository _textRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramBotClient _bot;
        private readonly QueueService _queueService;
        private readonly ILogger<JobService> _log;

        public JobService(IChatJobRepository chatJobRepository, IBackgroundJobClient backgroundJobClient, ILogger<JobService> logger, QueueService queueService, IChatRepository chatRepository, ITelegramBotClient bot, ITextRepository textRepository, IUserRepository userRepository)
        {
            _chatJobRepository = chatJobRepository;
            _backgroundJobClient = backgroundJobClient;
            _chatRepository = chatRepository;
            _queueService = queueService;
            _log = logger;
            _bot = bot;
            _textRepository = textRepository;
            _userRepository = userRepository;
        }

        public async Task<ChatJob> GetAsync(string chatJobId)
        {
            return await _chatJobRepository.GetAsync(chatJobId);
        }

        public async Task<ChatJob> CreateJobAsync(long chatId, string jobName, string cronExpression, string? queueId = null)
        {
            var chatJob = new ChatJob(chatId, jobName, cronExpression);
            SetNextRunTime(chatJob);
            chatJob = await _chatJobRepository.CreateAsync(chatJob);
            RecurringJob.AddOrUpdate(chatJob.JobId, () => ExecuteJob(chatJob), chatJob.CronExpression);
            _log.LogDebug("Created a new job with name {JobName} for chat {ChatId}", jobName, chatId);
            return chatJob;
        }

        public async Task<ChatJob> UpdateJobAsync(ChatJob updatedJob)
        {
            RecurringJob.AddOrUpdate(updatedJob.JobId, () => ExecuteJob(updatedJob), updatedJob.CronExpression);

            SetNextRunTime(updatedJob);
            await _chatJobRepository.UpdateAsync(updatedJob);
            _log.LogDebug("Updated job with name {JobName} for chat {ChatId}", updatedJob.JobName, updatedJob.ChatId);

            return updatedJob;
        }

        public async Task ExecuteJob(ChatJob job)
        {
            if (job.Interval > job.LastInterval)
            {
                job.LastInterval++;
                _log.LogDebug("The chat job has been rescheduled for the next week {lastInterval}/{interval}", job.LastInterval, job.Interval);
                return;
            }
            job.LastInterval = 1;
            _log.LogInformation("Executing chat job {chatJobId}", job.Id);

            var chat = await _chatRepository.GetByTelegramIdAsync(job.ChatId);

            // checking for an outdated chat 
            if (chat is null)
            {
                await DeleteJobAsync(job.Id);
                _log.LogInformation("Chat with id {chatId} was not found, the job was unscheduled", job.ChatId);
                return;
            }

            // checking for an outdated queue id
            if (job.QueueId is null)
            {
                await CreateNewQueueJob(chat);
                await UpdateJobAsync(job);
                return;
            }

            var queue = await _queueService.GetByIdAsync(job.QueueId);
            if (queue is null)
            {
                await CreateNewQueueJob(chat);
                if (job.QueueId is not null)
                {
                    _log.LogDebug("The passed queue identifier {queueId} was not found in the repository", job.QueueId);
                    job.QueueId = null;
                }
            } else
            {
                await LoadSavedQueueJob(chat, queue.Id);
            }

            await UpdateJobAsync(job);
        }

        public async Task DeleteJobAsync(string chatJobId)
        {
            var job = await _chatJobRepository.GetAsync(chatJobId);
            RecurringJob.RemoveIfExists(job.JobId);

            await _chatJobRepository.DeleteAsync(chatJobId);
            _log.LogInformation("Deleted chat job with Id {chatJobId}", chatJobId);
        }

        private void SetNextRunTime(ChatJob chatJob)
        {
            var cron = CronExpression.Parse(chatJob.CronExpression);
            chatJob.NextRunTimeUtc = cron.GetNextOccurrence(DateTime.UtcNow).Value.AddDays(7 * (chatJob.Interval - chatJob.LastInterval));
        }

        private async Task CreateNewQueueJob(Models.Chat chat)
        {
            var queue = await _queueService.CreateQueueAsync(chat.TelegramId, chat.DefaultQueueSize);

            if (queue is null)
            {
                _log.LogWarning("An error occurred when creating a queue for chat {id}, a null value was received", chat.TelegramId);
                return;
            }

            if (!string.IsNullOrEmpty(chat.CurrentQueueId) && !chat.SavedQueuesIds.Contains(chat.CurrentQueueId))
                await _queueService.DeleteQueueAsync(chat.CurrentQueueId);

            chat.CurrentQueueId = queue.Id;

            var msg = new MessageBuilder(chat)
                .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.ScheduledQueue))
                .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.CreatedQueue))
                .AddDefaultQueueMarkup(new List<Models.User>(new Models.User[chat.DefaultQueueSize]), chat.View);

            await DeleteLastMessageAndSendNew(chat, msg);
        }

        private async Task LoadSavedQueueJob(Models.Chat chat, string queueId)
        {
            var msg = new MessageBuilder(chat);

            chat.CurrentQueueId = queueId;
            chat.Mode = Models.Enums.ChatMode.Open;
            await _queueService.DoThreadSafeWorkOnQueueAsync(queueId, async (queue) =>
            {
                chat.DefaultQueueSize = queue.Size;
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg
                    .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.ScheduledQueue))
                    .AppendText($"{await _textRepository.GetValueAsync(TextKeys.CurrentQueue)} - {queue.Name}")
                    .AddDefaultQueueMarkup(users, chat.View);
            });
            await DeleteLastMessageAndSendNew(chat, msg);
        }

        private async Task DeleteLastMessageAndSendNew(Models.Chat chat, MessageBuilder msg)
        {
            try
            {
                await _bot.DeleteMessageAsync(chat.TelegramId, chat.LastMessageId);
            }
            catch (Exception ex) { }
            var result = await _bot.BuildAndSendAsync(msg);
            if (result is not null)
            {
                chat.LastMessageId = result.MessageId;
            }
            var a = await _chatRepository.UpdateAsync(chat);
        }
    }
}
