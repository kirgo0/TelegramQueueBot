using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Extensions.Logging;
//using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Commands;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UpdateHandler
    {
        protected ITelegramBotClient _bot;
        protected ILifetimeScope _scope;
        protected ILogger _logger;
        protected UpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger)
        {
            _bot = bot;
            _scope = scope;
            _logger = logger;
        }


        public abstract Task Handle(Update update);

        public virtual async Task RedirectHandle(Update update, string serviceMetaTag, Func<Update, object, Meta<UpdateHandler>, UpdateHandler> comparator, string errorMessage)
        {
            UpdateHandler handler = null;
            try
            {
                object value;
                var handlers = _scope.Resolve<IEnumerable<Meta<UpdateHandler>>>();
                foreach (var item in handlers)
                {
                    if (item.Metadata.TryGetValue(serviceMetaTag, out value))
                    {
                        handler = comparator.Invoke(update, value, item);
                        if (handler != null) break;
                    }
                }
                if (handler == null) return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, errorMessage);
            }
            try
            {
                await handler.Handle(update);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "some message");
                return;
            }
        }
    }

}
