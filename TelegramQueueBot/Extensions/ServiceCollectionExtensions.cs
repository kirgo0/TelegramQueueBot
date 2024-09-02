using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
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

        public static IServiceCollection AddMongoRepositoryWithCaching<TRepository, TCachedRepository, TEntity, IRepository>(
        this IServiceCollection services,
        Action<MemoryCacheEntryOptions> cacheOptionsBuilder)
        where TRepository : MongoRepository<TEntity>
        where TCachedRepository : CachedMongoRepository<TRepository, TEntity>, IRepository
        where TEntity : Entity, new()
        where IRepository : class
        {
            // Register the MongoRepository by its interface
            services.AddScoped<TRepository>();

            // Register the CachedMongoRepository by its interface
            services.AddScoped<IRepository>(provider =>
            {
                var innerRepository = provider.GetRequiredService<TRepository>();
                var logger = provider.GetRequiredService<ILogger<TCachedRepository>>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                var cacheOptions = new MemoryCacheEntryOptions();
                cacheOptionsBuilder.Invoke(cacheOptions);
                // Instantiate and return the cached repository
                return (TCachedRepository)Activator.CreateInstance(
                    typeof(TCachedRepository),
                    innerRepository,
                    logger,
                    cache,
                    cacheOptions
                );
            });

            return services;
        }

        public static IServiceCollection AddMongoRepositoryWithCaching<TRepository, TCachedRepository, TEntity, IRepository>(
        this IServiceCollection services,
        TimeSpan cacheDuration)
        where TRepository : MongoRepository<TEntity>
        where TCachedRepository : CachedMongoRepository<TRepository, TEntity>, IRepository
        where TEntity : Entity, new()
        where IRepository : class
        {
            // Register the MongoRepository by its interface
            services.AddScoped<TRepository>();

            // Register the CachedMongoRepository by its interface
            services.AddScoped<IRepository>(provider =>
            {
                var innerRepository = provider.GetRequiredService<TRepository>();
                var logger = provider.GetRequiredService<ILogger<TCachedRepository>>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                var cacheOptions = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = cacheDuration
                };
                
                // Instantiate and return the cached repository
                return (TCachedRepository)Activator.CreateInstance(
                    typeof(TCachedRepository),
                    innerRepository,
                    logger,
                    cache,
                    cacheOptions
                );
            });

            return services;
        }
    }
}
