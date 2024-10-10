using Autofac;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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
    [HandleAction(Common.Action.AcceptSwap)]
    public class AcceptSwapActionHandler : UpdateHandler
    {
        private ISwapRequestRepository _swapRequestRepository;
        private QueueService _queueService;
        public AcceptSwapActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<AcceptSwapActionHandler> logger, ISwapRequestRepository swapRequestRepository, QueueService queueService, IUserRepository userRepository) : base(bot, scope, logger)
        {
            NeedsUser = true;
            _swapRequestRepository = swapRequestRepository;
            _queueService = queueService;
            _userRepository = userRepository;
        }

        public override async Task Handle(Update update)
        {
            var action = GetAction(update);
            var actionData = action.Replace(Common.Action.AcceptSwap, string.Empty);
            var swapRequest = await _swapRequestRepository.GetAsync(actionData);

            if (swapRequest is null)
            {
                await SendSwapRequestIsOutdatedMessage();
                await _bot.DeleteMessageAsync(userTask.Result.TelegramId, update.CallbackQuery.Message.MessageId);
                return;
            }

            var swapResult = await _queueService.SwapUsersInPositionsAsync(
                swapRequest.QueueId,
                swapRequest.FirstTelegramId,
                swapRequest.FirstPosition,
                swapRequest.SecondTelegramId,
                swapRequest.SecondPosition
            );

            if(!swapResult)
            {
                await SendSwapRequestIsOutdatedMessage();
                await _swapRequestRepository.DeleteAsync(swapRequest.Id);
                return;
            }

            _log.LogInformation("Users {firstId} and {secondId} have been swapped", swapRequest.FirstTelegramId, swapRequest.SecondTelegramId);

            var firstUserTask = _userRepository.GetByTelegramIdAsync(swapRequest.FirstTelegramId);
            var secondUserTask = _userRepository.GetByTelegramIdAsync(swapRequest.SecondTelegramId);
            
            await Task.WhenAll(firstUserTask, secondUserTask);
            
            var firstMsg = new MessageBuilder()
                .SetChatId(swapRequest.FirstTelegramId)
                .AppendTextLine(TextResources.GetValue(TextKeys.SwapRequestCaption))
                .AppendTextLine()
                .AppendTextFormat(TextResources.GetValue(TextKeys.SwapRequestSuccess), secondUserTask.Result.UserName);

            var secondMsg = new MessageBuilder()
                .SetChatId(swapRequest.SecondTelegramId)
                .AppendTextLine(TextResources.GetValue(TextKeys.SwapRequestCaption))
                .AppendTextLine()
                .AppendTextFormat(TextResources.GetValue(TextKeys.SwapRequestSuccess), firstUserTask.Result.UserName);

            await Task.WhenAll(_bot.BuildAndSendAsync(firstMsg), _bot.BuildAndSendAsync(secondMsg));

            await _swapRequestRepository.DeleteAsync(swapRequest.Id);
        }

        protected async Task SendSwapRequestIsOutdatedMessage()
        {
            var user = await userTask;
            var msg = new MessageBuilder()
                .SetChatId(user.TelegramId)
                .AppendText(TextResources.GetValue(TextKeys.SwapRequestOutdated));
            await _bot.BuildAndSendAsync(msg);
        }
    }
}
