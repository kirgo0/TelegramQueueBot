using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services
{
    public class MongoQueueSaveBackgroundService : BackgroundService
    {
        private static int saveOperationCounter = 0;
        private static int recievedRequests = 0;
        private QueueService _queueService;
        private ICachedQueueRepository _cachedQueueRepository;
        private ConcurrentDictionary<string, Queue> _queues = new();
        private ILogger _log;
        public TimeSpan SaveDelay { get; set; }

        public MongoQueueSaveBackgroundService(ICachedQueueRepository cachedQueueRepository, QueueService queueService, ILogger<MongoQueueSaveBackgroundService> log, TimeSpan saveDelay)
        {
            _cachedQueueRepository = cachedQueueRepository;
            _cachedQueueRepository.QueueUpdateEvent += OnSaveEvent;
            _queueService = queueService;
            _log = log;
            SaveDelay = saveDelay;
        }

        private void OnSaveEvent(object? sender, QueueUpdatedEventArgs e)
        {
            _queues.AddOrUpdate(e.Queue.Id, e.Queue, (_, queue) =>
            {
                return queue;
            });
            recievedRequests++;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var key in _queues.Keys)
                {
                    if (_queues.TryRemove(key, out var value))
                    {
                        saveOperationCounter++;
                        await _queueService.DoThreadSafeWorkOnQueueAsync(key, async (queue) =>
                        {
                            _log.LogInformation("[SAVE:{saveOperationCounter}] Start processing for {key}", saveOperationCounter, key);
                            var staticCounter = saveOperationCounter;
                            var result = await _cachedQueueRepository.InnerRepository.UpdateAsync(value);
                            _log.LogInformation("[SAVE:{staticCounter}] Processing result: {result}", staticCounter, result);
                        });
                    }
                }
                await Task.Delay(SaveDelay);
            }
        }
    }
}
