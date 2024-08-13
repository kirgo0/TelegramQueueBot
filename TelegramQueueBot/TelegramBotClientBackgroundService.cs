using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

//using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot
{
    public class TelegramBotClientBackgroundService : BackgroundService
    {
        private ITelegramBotClient _bot;
        private ReceiverOptions _receiverOptions;
        private ILogger<TelegramBotClientBackgroundService> _logger;
        private ILifetimeScope _scope;

        public TelegramBotClientBackgroundService(ITelegramBotClient bot, ILogger<TelegramBotClientBackgroundService> logger, ILifetimeScope scope)
        {
            _bot = bot;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            };
            _logger = logger;
            _scope = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram bot starts responding to requests");
            while (true)
            {
                await _bot.ReceiveAsync(HandleUpdate, HandleError, _receiverOptions, stoppingToken);
                await Task.Delay(100);
            }
        }

        async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            UpdateHandler handler = null;
            try
            {
                object type;
                var handlers = _scope.Resolve<IEnumerable<Meta<UpdateHandler>>>();
                foreach (var item in handlers)
                {
                    if (item.Metadata.TryGetValue(Metatags.HandleType, out type))
                    {
                        if ((UpdateType)type == update.Type)
                        {
                            handler = item.Value;
                        }
                    }
                }
                if (handler == null) return;
                await handler.Handle(update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while resolving update handler for type {type}", update.Type);
            }
        }
        async Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Telegram API request exception: {message}", ex.Message);
        }
    }
}
