﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services
{
    public class TelegramQueueRenderBackgroundService : BackgroundService
    {
        private ITelegramBotClient _bot;

        private QueueService _queueService;
        private IUserRepository _userRepository;
        private IChatRepository _chatRepository;
        private ICachedQueueRepository _cachedQueueRepository;

        private ConcurrentDictionary<string, Queue> _queues = new();
        private TimeSpan _delay;

        private ILogger _log;

        public TelegramQueueRenderBackgroundService(
            ITelegramBotClient bot, 
            QueueService queueService, 
            IUserRepository userRepository, 
            IChatRepository chatRepository, 
            ICachedQueueRepository cachedQueueRepository, 
            TimeSpan delay, 
            ILogger log)
        {
            _bot = bot;
            _queueService = queueService;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _cachedQueueRepository = cachedQueueRepository;
            _delay = delay;
            _log = log;
            _cachedQueueRepository.QueueUpdateEvent += OnQueueUpdatedEvent;
        }

        public void OnQueueUpdatedEvent(object? sender, QueueUpdatedEventArgs e)
        {
            _queues.AddOrUpdate(e.Queue.Id, e.Queue, (key, _) => e.Queue);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var key in _queues.Keys)
                {
                    if (_queues.TryRemove(key, out var value))
                    {
                        await Render(value);
                    }
                }
                await Task.Delay(_delay, stoppingToken);
            }
        }

        protected async Task Render(Queue queue)
        {
            await _queueService.DoThreadSafeWorkOnQueueAsync(queue.Id, async () =>
            {
                var namesTask = _userRepository.GetRangeByTelegramIdsAsync(queue.List);
                var chatTask = _chatRepository.GetByTelegramIdAsync(queue.ChatId);

                var msg = new MessageBuilder(await chatTask)
                    .AppendText("Капець воно само сосе")
                    .AddDefaultQueueMarkup(await namesTask);

                await _bot.BuildAndSendAsync(msg);
            });

        }
    }
}
