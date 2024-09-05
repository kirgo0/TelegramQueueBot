﻿using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;

namespace TelegramQueueBot.UpdateHandlers.Others
{
    public class ScheduledQueueJobHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly ITelegramBotClient _bot;
        private readonly ITextRepository _textRepository;
        private readonly IUserRepository _userRepository;
        private readonly QueueService _queueService;
        private readonly ILogger _log;
        public ScheduledQueueJobHandler(IChatRepository chatRepository, QueueService queueService, ILogger<ScheduledQueueJobHandler> log, IUserRepository userRepository, ITextRepository textRepository, ITelegramBotClient bot)
        {
            _bot = bot;
            _log = log;
            _chatRepository = chatRepository;
            _queueService = queueService;
            _userRepository = userRepository;
            _textRepository = textRepository;
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

            // checking for an outdated queue id
            if (job.QueueId is null)
            {
                await CreateNewQueueJob(chat);
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
                .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.ScheduledQueue))
                .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.CreatedQueue))
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
                    .AppendTextLine(await _textRepository.GetValueAsync(TextKeys.ScheduledQueue))
                    .AppendText($"{await _textRepository.GetValueAsync(TextKeys.CurrentQueue)} - {queue.Name}")
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
            var a = await _chatRepository.UpdateAsync(chat);
        }
    }
}