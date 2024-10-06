using Autofac;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Common;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.UpdateHandlers.Callbacks
{
    [HandleAction(Common.Action.DenySwap)]
    public class DenySwapActionHandler : UpdateHandler
    {
        private ISwapRequestRepository _swapRequestRepository;
        public DenySwapActionHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger<DenySwapActionHandler> logger, ISwapRequestRepository swapRequestRepository) : base(bot, scope, logger)
        {
            NeedsUser = true;
            _swapRequestRepository = swapRequestRepository;
        }

        public override async Task Handle(Update update)
        {
            var action = GetAction(update);
            var actionData = action.Replace(Common.Action.DenySwap, string.Empty);

            var swapRequest = await _swapRequestRepository.GetAsync(actionData);

            if (swapRequest is null)
            {
                var user = await userTask;
                var errorMsg = new MessageBuilder()
                    .SetChatId(user.TelegramId)
                    .AppendText(TextResources.GetValue(TextKeys.SwapRequestOutdated));
                await _bot.BuildAndSendAsync(errorMsg);
                await _bot.DeleteMessageAsync(user.TelegramId, update.CallbackQuery.Message.MessageId);
                return;
            }

            var msg = new MessageBuilder()
                .AppendText(TextResources.GetValue(TextKeys.SwapRequestDenied));

            await SendToManyUsersAsync(msg, swapRequest.FirstTelegramId, swapRequest.SecondTelegramId);

            await _swapRequestRepository.DeleteAsync(swapRequest.Id);
        }
    }
}
