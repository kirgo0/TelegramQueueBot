using Cronos;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services
{
    public class JobService
    {
        private readonly IChatJobRepository _chatJobRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<JobService> _logger;

        public JobService(IChatJobRepository chatJobRepository, IBackgroundJobClient backgroundJobClient, ILogger<JobService> logger)
        {
            _chatJobRepository = chatJobRepository;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task<ChatJob> GetAsync(string chatJobId)
        {
            return await _chatJobRepository.GetAsync(chatJobId);
        }

        public async Task<ChatJob> CreateJobAsync(long chatId, string jobName, string cronExpression, string? queueId = null)
        {
            var chatJob = new ChatJob(chatId, jobName, cronExpression);

            RecurringJob.AddOrUpdate(chatJob.JobId, () => ExecuteJob(chatJob), chatJob.CronExpression);

            SetNextRunTime(chatJob);

            chatJob = await _chatJobRepository.CreateAsync(chatJob);
            _logger.LogDebug("Created a new job with name {JobName} for chat {ChatId}", jobName, chatId);

            return chatJob;
        }

        public async Task<ChatJob> UpdateJobAsync(ChatJob updatedJob)
        {
            RecurringJob.AddOrUpdate(updatedJob.JobId, () => ExecuteJob(updatedJob), updatedJob.CronExpression);

            SetNextRunTime(updatedJob);
            await _chatJobRepository.UpdateAsync(updatedJob);
            _logger.LogDebug("Updated job with name {JobName} for chat {ChatId}", updatedJob.JobName, updatedJob.ChatId);

            return updatedJob;
        }

        public async Task ExecuteJob(ChatJob job)
        {
            if(job.Interval > job.LastInterval)
            {
                job.LastInterval++;
                _logger.LogDebug("The chat job has been rescheduled for the next week {lastInterval}/{interval}", job.LastInterval, job.Interval);
                return;
            }
            job.LastInterval = 1;
            await UpdateJobAsync(job);
            _logger.LogInformation("Executing chat job {chatJobId}", job.Id);
            // TODO: send message logic & clear out dated data
        }

        public async Task DeleteJobAsync(string chatJobId)
        {
            var job = await _chatJobRepository.GetAsync(chatJobId);
            RecurringJob.RemoveIfExists(job.JobId);

            await _chatJobRepository.DeleteAsync(chatJobId);
            _logger.LogInformation("Deleted chat job with Id {chatJobId}", chatJobId);
        }

        private void SetNextRunTime(ChatJob chatJob)
        {
            var cron = CronExpression.Parse(chatJob.CronExpression);
            chatJob.NextRunTimeUtc = cron.GetNextOccurrence(DateTime.UtcNow).Value.AddDays(7 * (chatJob.Interval - chatJob.LastInterval));
        }

    }
}
