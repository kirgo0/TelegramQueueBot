using Cronos;
using Hangfire;
using Microsoft.Extensions.Logging;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Others;

namespace TelegramQueueBot.Services
{
    public class JobService
    {
        private readonly IChatJobRepository _chatJobRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly ScheduledQueueJobHandler _handler;
        private readonly ILogger<JobService> _log;

        public JobService(IChatJobRepository chatJobRepository, ILogger<JobService> log, IServiceProvider serviceProvider, ScheduledQueueJobHandler handler)
        {
            _chatJobRepository = chatJobRepository;
            _log = log;
            _serviceProvider = serviceProvider;
            _handler = handler;
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
            var result = await _handler.Handle(job);
            if (result)
                await UpdateJobAsync(job);
            else
                await DeleteJobAsync(job.Id);
            _log.LogInformation("Finished processing chat job {chatJobId} successfully", job.Id);
        }

        public async Task DeleteJobAsync(string chatJobId)
        {
            var job = await _chatJobRepository.GetAsync(chatJobId);
            RecurringJob.RemoveIfExists(job.JobId);

            if (await _chatJobRepository.DeleteAsync(chatJobId))
            {
                _log.LogInformation("Deleted chat job with Id {chatJobId}", chatJobId);
            }
        }

        private void SetNextRunTime(ChatJob chatJob)
        {
            var cron = CronExpression.Parse(chatJob.CronExpression);
            chatJob.NextRunTimeUtc = cron.GetNextOccurrence(DateTime.UtcNow).Value.AddDays(7 * (chatJob.Interval - chatJob.LastInterval));
        }

    }
}
