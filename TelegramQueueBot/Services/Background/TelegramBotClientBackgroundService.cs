using Autofac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DefaultUpdateHandler = TelegramQueueBot.UpdateHandlers.DefaultUpdateHandler;

namespace TelegramQueueBot.Services.Background
{
    public class TelegramBotClientBackgroundService : BackgroundService
    {
        private ITelegramBotClient _bot;
        private ReceiverOptions _receiverOptions;
        private ILogger<TelegramBotClientBackgroundService> _logger;
        private DefaultUpdateHandler _defaultUpdateHandler;


        public TelegramBotClientBackgroundService(ITelegramBotClient bot, ILogger<TelegramBotClientBackgroundService> logger, ILifetimeScope scope, DefaultUpdateHandler defaultUpdateHandler)
        {
            _bot = bot;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            };
            _logger = logger;
            _defaultUpdateHandler = defaultUpdateHandler;
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
            await _defaultUpdateHandler.Handle(update);
        }
        async Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Telegram API request exception: {message}", ex.Message);
        }
    }
}
