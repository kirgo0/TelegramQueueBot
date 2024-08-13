using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Extensions.Logging;
using QueueCore;
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
        protected ILogger _log;
        protected IQueueService _queueService;
        protected UpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger)
        {
            _bot = bot;
            _scope = scope;
            _log = logger;
        }


        public abstract Task Handle(Update update);

        public virtual async Task RedirectHandle(Update update, string serviceMetaTag, Func<Update, object, Meta<UpdateHandler>, bool> comparator, string resolvingErrorMessage, params object[] resolveErrorParams)
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
                        if(comparator.Invoke(update, value, item))
                        {
                            handler = item.Value;
                            break;
                        }
                        if (handler is not null) break;
                    }
                }
                if (handler is null) throw new Exception();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, resolvingErrorMessage, resolveErrorParams);
                return;
            }
            try
            {
                await handler.Handle(update);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while handling a request with a  {handler}", handler);
                return;
            }
        }
    }

}
