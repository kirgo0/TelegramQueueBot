using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
        private int UsersToNotifyCount = 2;
        private QueueService _queueService;
        private IUserRepository _userRepository;
        private IChatRepository _chatRepository;
        private ICachedQueueRepository _cachedQueueRepository;

        private ConcurrentDictionary<string, Queue> _queues = new();
        private ConcurrentDictionary<string, List<long>> _queueSnapshots = new();
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

                    bool sendNewMessage = false;
                    if (chat.Mode is Models.Enums.ChatMode.CallingUsers)
                    {
                        try
                        {
                            await _bot.DeleteMessageAsync(chat.TelegramId, chat.LastMessageId);
                        }
                        catch (ApiRequestException ex) { }
                        sendNewMessage = true;
                    } else
                    {
                        try
                        {
                            await _bot.BuildAndEditAsync(msg);
                        }
                        catch(ApiRequestException ex)
                        {
                            if(ex.Message.Equals("Bad Request: message to edit not found"))
                            {
                                sendNewMessage = true;
                            }
                        }
                    }
                    if (sendNewMessage)
                    {
                        var message = await _bot.BuildAndSendAsync(msg);
                        chat.LastMessageId = message.MessageId;
                        await _chatRepository.UpdateAsync(chat);
                    }
                    await NotifyFirstUsersIfOrderChanged(chat, queue, usersTask.Result);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "An error occured while rendering queue {queueId} for chat {chatId}", queue.Id, queue.ChatId);
                }
            });
        }

        private async Task NotifyFirstUsersIfOrderChanged(Chat chat, Queue queue, List<User> users)
        {
            if (chat is null || queue is null) return;
            if (chat.Mode is not Models.Enums.ChatMode.CallingUsers)
            {
                _queueSnapshots.TryRemove(queue.Id, out var _);
                return;
            }

            var queueSnapshot = _queueSnapshots.GetValueOrDefault(queue.Id);
            
            await SendNotificationsToNewUsers(
                chat, 
                GetNFirst(users, queueSnapshot, UsersToNotifyCount),
                GetNFirst(users, queue.List, UsersToNotifyCount)
            );

            _queueSnapshots.AddOrUpdate(queue.Id, (key) => new Queue(queue).List, (key, old) => old = new Queue(queue).List);

        }
        private async Task SendNotificationsToNewUsers(Chat chat, List<User?> previousOrder, List<User?> currentOrder)
        {
            if(previousOrder is null || currentOrder is null) throw new ArgumentNullException(nameof(previousOrder));
            if(previousOrder.Count != currentOrder.Count) throw new ArgumentException($"{nameof(previousOrder)} list count must be equal to {nameof(currentOrder)} list count");

            var notifyTasks = new List<Task>();
            currentOrder = currentOrder.Where(x => x is not null).ToList();
            for (int i = 0; i < currentOrder.Count; i++)
            {
                var currentUser = currentOrder[i];
                var previousUser = previousOrder[i];
                if (currentUser is null) continue;
                if(currentUser.TelegramId != previousUser?.TelegramId)
                {
                    if(i == 0)
                    {
                        var leaveBtn = new InlineKeyboardButton(TextResources.GetValue(TextKeys.LeaveBtn));
                        leaveBtn.CallbackData = $"{Common.Action.Leave}{chat.TelegramId}";
                        notifyTasks.Add(SendUserMessageAsync(
                            $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{TextResources.GetValue(TextKeys.FirstUserInQueue)}",
                            chat.Id,
                            currentUser,
                            markup: new InlineKeyboardMarkup(leaveBtn)
                            ));
                    }
                    else
                    {
                        notifyTasks.Add(SendUserMessageAsync(
                            $"{TextResources.GetValue(TextKeys.QueueIsCallingUsers)}\n\n{string.Format(TextResources.GetValue(TextKeys.NextUserInQueue), i + 1)}",
                            chat.Id, 
                            currentUser
                            )
                        );
                    }
                }
            }
            await Task.WhenAll(notifyTasks);
        }

        private async Task SendUserMessageAsync(string message, string chatId, User user, ParseMode parseMode = ParseMode.Html, InlineKeyboardMarkup markup = null)
        {
            if (!(user).SendNotifications) return;
            if (!user.ChatIds.TryGetValue(chatId, out bool enabled) || !enabled) return;

            await _bot.SendTextMessageAsync(
                user.TelegramId,
                message,
                parseMode: parseMode,
                replyMarkup: markup
            );
        }

        private List<User?> GetNFirst(List<User> users, List<long> queueList, int n)
        {
            if(users is null) throw new ArgumentNullException(nameof(users));

            if(queueList is null) return Enumerable.Repeat<User?>(null, n).ToList();

            var firstNUsers = queueList.Where(x => x != 0).Take(n).ToList();
            if(firstNUsers.Count < n)
            {
                firstNUsers.AddRange(Enumerable.Repeat<long>(0, n - firstNUsers.Count));
            }
            return firstNUsers.Select(id => users.FirstOrDefault(u => u?.TelegramId == id)).ToList();
        }
    }
}
