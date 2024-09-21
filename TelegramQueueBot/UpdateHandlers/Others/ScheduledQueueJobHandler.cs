using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Others
{
    public class ScheduledQueueJobHandler : UserNotifyingUpdateHandler
    {
        private readonly QueueService _queueService;
        public ScheduledQueueJobHandler(IChatRepository chatRepository, QueueService queueService, ILogger<ScheduledQueueJobHandler> log, IUserRepository userRepository, ITelegramBotClient bot, ILifetimeScope scope) : base(bot, scope, log)
        {
            _chatRepository = chatRepository;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ChatJob job)
        {
            var chat = await _chatRepository.GetByTelegramIdAsync(job.ChatId);

            // checking for an outdated chat 
            if (chat is null)
            {
                _log.LogInformation("Chat with id {chatId} was not found, the job was unscheduled", job.ChatId);
                return false;
            }

            var usersToNotify = await _userRepository.GetUsersWithAllowedNotificationsAsync(chat.Id);
            var usersTasks = new List<Task>();

            if (usersToNotify.Count != 0)
            {
                var chatName = (await _bot.GetChatAsync(chat.TelegramId)).Title;
                var msg = new MessageBuilder().AppendTextFormat(TextResources.GetValue(TextKeys.ScheduledQueueAppeared), chatName, job.JobName);
                foreach (var user in usersToNotify)
                {
                    usersTasks.Add(SendUserMessageAsync(user.TelegramId, msg));
                }
            }

            // checking for an outdated queue id
            if (job.QueueId is null)
            {
                await CreateNewQueueJob(chat);
                await Task.WhenAll(usersTasks);
                return true;
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
            }
            else
            {
                await LoadSavedQueueJob(chat, queue.Id);
            }
            await Task.WhenAll(usersTasks);
            return true;
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
                .AppendTextLine(TextResources.GetValue(TextKeys.ScheduledQueue))
                .AppendTextLine(TextResources.GetValue(TextKeys.CreatedQueue))
                .AddDefaultQueueMarkup(new List<User>(new User[chat.DefaultQueueSize]), chat.View);

            await DeleteLastMessageAndSendNew(chat, msg);
        }

        private async Task LoadSavedQueueJob(Chat chat, string queueId)
        {
            var msg = new MessageBuilder(chat);

            chat.CurrentQueueId = queueId;
            chat.Mode = Models.Enums.ChatMode.Open;
            await _queueService.DoThreadSafeWorkOnQueueAsync(queueId, async (queue) =>
            {
                chat.DefaultQueueSize = queue.Size;
                var users = await _userRepository.GetByTelegramIdsAsync(queue.List);
                msg
                    .AppendTextLine(TextResources.GetValue(TextKeys.ScheduledQueue))
                    .AppendText($"{TextResources.GetValue(TextKeys.CurrentQueue)} - {queue.Name}")
                    .AddDefaultQueueMarkup(users, chat.View);
            });
            await DeleteLastMessageAndSendNew(chat, msg);
        }

        private async Task DeleteLastMessageAndSendNew(Chat chat, MessageBuilder msg)
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
            await _chatRepository.UpdateAsync(chat);
        }

        public override Task Handle(Telegram.Bot.Types.Update update)
        {
            throw new NotImplementedException();
        }
    }
}
