using Autofac;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Common.Action.Dequeue)]
    public class DequeueActionHandler : UpdateHandler
    {
        private QueueService _queueService;
        private ISwapRequestRepository _swapRequestRepository;
        public DequeueActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DequeueActionHandler> logger, QueueService queueService, ISwapRequestRepository swapRequestRepository) : base(bot, scope, logger)
        {
            GroupsOnly = true;
            NeedsUser = true;
            NeedsChat = true;
            _queueService = queueService;
            _swapRequestRepository = swapRequestRepository;
        }

        public override async Task Handle(Update update)
        {
            var chat = await chatTask;
            var user = await userTask;
            var action = GetAction(update);
            _log.LogInformation("User {id} requested {data}", user.TelegramId, action);
            var actionData = action.Replace(Common.Action.Dequeue, string.Empty);

            if (!int.TryParse(actionData, out int actionUserId))
            {
                _log.LogError("The id {id} in the chat {chatId} is not parsed", actionData, chat.TelegramId);
            }

            try
            {
                // dequeing user
                if (user.TelegramId == actionUserId)
                {
                    if (await _queueService.GetQueueCountAsync(chat.CurrentQueueId) == 1 && chat.Mode == Models.Enums.ChatMode.CallingUsers)
                    {
                        // dequeue last user and send ending calling user message to chat

                        var msg = new MessageBuilder(chat);
                        var result = await _queueService.DequeueAsync(chat.CurrentQueueId, user.TelegramId);

                        if (!result) return;

                        chat.Mode = Models.Enums.ChatMode.Open;

                        msg.AppendTextLine(TextResources.GetValue(TextKeys.QueueEndedCallingUsers));
                        await _bot.BuildAndSendAsync(msg);
                    }
                    else
                    {
                        await _queueService.DequeueAsync(chat.CurrentQueueId, user.TelegramId);
                        return;
                    }
                }
                // swaping two users
                else
                {
                    if (!user.IsAuthorized)
                    {
                        await _bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, TextResources.GetValue(TextKeys.NeedToBeAuthorizedForSwap), cacheTime: 3);
                        return;
                    }
                    await HandleSwapRequest(update, chat, user, actionUserId);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An error occured while dequeing user {userid} in chat {chatId}, queue {queueId}", user.TelegramId, chat.TelegramId, chat.CurrentQueueId);
            }
        }

        public async Task HandleSwapRequest(Update update, Models.Chat chat, Models.User firstUser, long secondUserId)
        {
            var secondUser = await _userRepository.GetByTelegramIdAsync(secondUserId);

            if (secondUser is null || !secondUser.IsAuthorized)
            {
                var userMsg = new MessageBuilder()
                    .SetChatId(firstUser.TelegramId)
                    .AppendTextFormat(TextResources.GetValue(TextKeys.UserNotAuthorized), secondUser?.UserName);
                await _bot.BuildAndSendAsync(userMsg);
                return;
            }


            int[] userPositions = new int[2];
            userPositions[0] = -1;
            userPositions[1] = -1;

            await _queueService.DoThreadSafeWorkOnQueueAsync(chat.CurrentQueueId, (queue) =>
            {
                for (int i = 0; i < queue.Size; i++)
                {
                    if (userPositions[0] != -1 && userPositions[1] != -1) break;
                    if (queue[i] == firstUser.TelegramId) userPositions[0] = i;
                    if (queue[i] == secondUserId) userPositions[1] = i;
                }
            });

            if (userPositions[0] == -1 || userPositions[1] == -1)
            {
                return;
            }
            
            var swapRequest = new SwapRequest()
            {
                QueueId = chat.CurrentQueueId,
                FirstTelegramId = firstUser.TelegramId,
                FirstPosition = userPositions[0],
                SecondTelegramId = secondUserId,
                SecondPosition = userPositions[1]
            };

            await _swapRequestRepository.CreateOrReplaceAsync(swapRequest);

            var firstMsg = new MessageBuilder()
              .SetChatId(firstUser.TelegramId)
              .AppendTextLine(TextResources.GetValue(TextKeys.SwapRequestCaption))
              .AppendTextLine()
              .AppendTextFormat(TextResources.GetValue(TextKeys.SwapRequestSendedFirstUser), secondUser.UserName)
              .AddButton(TextResources.GetValue(TextKeys.DenyBtn), callbackData: $"{Common.Action.DenySwap}{swapRequest.Id}");

            var secondMsg = new MessageBuilder()
                .SetChatId(secondUserId)
                .AppendTextLine(TextResources.GetValue(TextKeys.SwapRequestCaption))
                .AppendTextLine()
                .AppendTextFormat(TextResources.GetValue(TextKeys.SwapRequestSendedSecondUser), firstUser.UserName, userPositions[1], userPositions[0])
                .AddButton(TextResources.GetValue(TextKeys.DoneBtn), callbackData: $"{Common.Action.AcceptSwap}{swapRequest.Id}")
                .AddButton(TextResources.GetValue(TextKeys.DenyBtn), callbackData: $"{Common.Action.DenySwap}{swapRequest.Id}");

          
            await Task.WhenAll(_bot.BuildAndSendAsync(firstMsg), _bot.BuildAndSendAsync(secondMsg));
        }
    }
}
