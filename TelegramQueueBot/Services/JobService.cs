using Hangfire;
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

        public async Task CreateOrUpdateJobAsync(long chatId, string jobName, string cronExpression)
        {
            // Створити або знайти існуючу роботу для даного чату
            var chatJob = await _chatJobRepository.GetAsync(jobName);
            if (chatJob == null)
            {
                chatJob = new ChatJob
                {
                    ChatId = chatId,
                    JobName = jobName,
                    CronExpression = cronExpression,
                    LastRunTime = DateTime.UtcNow,
                    NextRunTime = DateTime.UtcNow
                };

                await _chatJobRepository.CreateAsync(chatJob);
                _logger.LogInformation("Created a new job with name {JobName} for chat {ChatId}", jobName, chatId);
            }
            else
            {
                chatJob.CronExpression = cronExpression;
                await _chatJobRepository.UpdateAsync(chatJob);
                _logger.LogInformation("Updated job with name {JobName} for chat {ChatId}", jobName, chatId);
            }

            RecurringJob.AddOrUpdate(chatJob.JobName, () => ExecuteJob(chatJob.Id), chatJob.CronExpression);
        }

        public void ExecuteJob(string jobId)
        {
            _logger.LogInformation("Executing job {jobId}", jobId);
        }

        public async Task DeleteJobAsync(string jobId)
        {
            // Видалити роботу з Hangfire
            var job = await _chatJobRepository.GetAsync(jobId);
            RecurringJob.RemoveIfExists(job.JobName);

            // Видалити роботу з бази даних
            await _chatJobRepository.DeleteAsync(jobId);
            _logger.LogInformation("Deleted job with Id {jobId}", jobId);
        }
    }
}
