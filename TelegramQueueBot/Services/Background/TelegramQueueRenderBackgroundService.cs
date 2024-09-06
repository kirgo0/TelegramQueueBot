using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services.Background
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
            ILogger log
            )
        {
            _bot = bot;
            _log = log;
            _queueService = queueService;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _cachedQueueRepository = cachedQueueRepository;
            _delay = delay;
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
                try
                {
                    var usersTask = _userRepository.GetByTelegramIdsAsync(queue.List);
                    var chat = await _chatRepository.GetByTelegramIdAsync(queue.ChatId);

                    var msg = new MessageBuilder(chat);

                    msg.AppendModeTitle(chat);

                    msg
                        .AppendText(TextResources.GetValue(TextKeys.CurrentQueue))
                        .AddDefaultQueueMarkup(await usersTask, chat.View);

                    await _bot.BuildAndEditAsync(msg);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "An error occured while rendering queue {queueId} for chat {chatId}", queue.Id, queue.ChatId);
                }
            });

        }
    }
}
