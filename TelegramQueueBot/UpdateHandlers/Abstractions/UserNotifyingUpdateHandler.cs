using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UserNotifyingUpdateHandler : UpdateHandler
    {
        protected UserNotifyingUpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger) : base(bot, scope, logger)
        {
        }
        protected async Task NotifyUsersIfOrderChanged(List<long> previousOrder, List<long> currentOrder)
        {

            var notifyTasks = new List<Task>();

            for (int i = 0; i < currentOrder.Count; i++)
            {
                long userId = (previousOrder.Count - 1 >= i && currentOrder[i] != previousOrder[i]) || previousOrder.Count - 1 < i
                    ? currentOrder[i]
                    : 0;
                if (i == 0 && userId != 0)
                {
                    notifyTasks.Add(NotifyUserAsync(TextResources.GetValue(TextKeys.FirstUserInQueue), userId));
                }
                else if (userId != 0)
                {
                    notifyTasks.Add(
                        NotifyUserAsync(
                            string.Format(TextResources.GetValue(TextKeys.NextUserInQueue), i + 1),
                            userId)
                    );
                }
            }
            await Task.WhenAll(notifyTasks);

        }

    }
}
