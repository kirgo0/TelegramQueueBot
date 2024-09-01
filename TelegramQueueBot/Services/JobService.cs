using Cronos;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.Logging;
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
            var chatJob = new ChatJob
            {
                ChatId = chatId,
                QueueId = queueId,
                JobId = Guid.NewGuid().ToString(),
                JobName = jobName,
                CronExpression = cronExpression,
                Interval = 1,
                LastRunTime = DateTime.UtcNow
            };

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

        public void ExecuteJob(ChatJob job)
        {
            _logger.LogInformation("Executing job {jobId}", job.Id);
        }

        public async Task DeleteJobAsync(string jobId)
        {
            var job = await _chatJobRepository.GetAsync(jobId);
            RecurringJob.RemoveIfExists(job.JobId);

            await _chatJobRepository.DeleteAsync(jobId);
            _logger.LogInformation("Deleted job with Id {jobId}", jobId);
        }

        private void SetNextRunTime(ChatJob chatJob)
        {
            var cron = CronExpression.Parse(chatJob.CronExpression);
            chatJob.NextRunTime = cron.GetNextOccurrence(chatJob.LastRunTime).Value;
        }
    }
}
