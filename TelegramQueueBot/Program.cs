using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Context;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Models;
using TelegramQueueBot.Modules;
using TelegramQueueBot.Repository.Implementations;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            //logging.AddConfiguration(context.Configuration);
            logging.AddSerilog(Log.Logger, dispose: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddMemoryCache();

            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton(provider =>
            {
                return new QueueService(provider.GetRequiredService<ICachedQueueRepository>());
            });

            TimeSpan cacheDuration = TimeSpan.FromMinutes(30);

            services.AddMongoRepositoryWithCaching<MongoUserRepository, CachedMongoUserRepository, User, IUserRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoChatRepository, CachedMongoChatRepository, Chat, IChatRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoTextRepository, CachedMongoTextRepository, Text, ITextRepository>(TimeSpan.FromMinutes(60));
            services.AddMongoRepositoryWithCaching<MongoQueueRepository, CachedMongoQueueRepository, Queue, ICachedQueueRepository>(TimeSpan.FromMinutes(10));

            services.AddMongoQueueSaveBackgroundService(TimeSpan.FromSeconds(1));
            services.AddTelegramQueueRenderBackgroundService(TimeSpan.FromMilliseconds(1000));

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(context.Configuration.GetSection("TelegramBotOptions")["Token"]));
            services.AddHostedService<TelegramBotClientBackgroundService>();
        })
        .ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
        {
            containerBuilder.RegisterModule<CommandsModule>();
            containerBuilder.RegisterModule<HandlersModule>();
            containerBuilder.RegisterModule<ActionsModule>();
        });

    using IHost host = builder.Build();
    await host.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Bot initialization error");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

