using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;

namespace TelegramQueueBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegramQueueRenderBackgroundService(
            this IServiceCollection services,
            TimeSpan delay)
        {
            services.AddSingleton(provider =>
                new TelegramQueueRenderBackgroundService(
                    provider.GetRequiredService<ITelegramBotClient>(),
                    provider.GetRequiredService<QueueService>(),
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<IChatRepository>(),
                    provider.GetRequiredService<ITextRepository>(),
                    provider.GetRequiredService<ICachedQueueRepository>(),
                    delay,
                    provider.GetRequiredService<ILogger<TelegramQueueRenderBackgroundService>>()
                    ));

            services.AddHostedService(provider => provider.GetRequiredService<TelegramQueueRenderBackgroundService>());
            return services;
        }


        public static IServiceCollection AddMongoQueueSaveBackgroundService(
            this IServiceCollection services,
            TimeSpan delay)
        {
            services.AddSingleton(provider =>
                new MongoQueueSaveBackgroundService(
                    provider.GetRequiredService<ICachedQueueRepository>(),
                    provider.GetRequiredService<QueueService>(),
                    provider.GetRequiredService<ILogger<MongoQueueSaveBackgroundService>>(),
                    delay)
                    );

            services.AddHostedService(provider => provider.GetRequiredService<MongoQueueSaveBackgroundService>());
            return services;
        }
    }
}
