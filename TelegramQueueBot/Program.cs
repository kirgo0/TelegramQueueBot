using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using TelegramQueueBot.Common;
using TelegramQueueBot.Data.Abstraction;
using Data.Context;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Modules;
using TelegramQueueBot.Repository.Implementations;
using TelegramQueueBot.Repository.Implementations.Cached;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;
using TelegramQueueBot.Services.Background;
using TelegramQueueBot.UpdateHandlers.Others;
using DotNetEnv;

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
            config.AddJsonFile($"appsettings.json", optional: false);
            config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true);
        })
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddMemoryCache();

            Env.Load();
            var envVariables = new Dictionary<string, string>
            {
                { "MONGO_CONNECTION", Environment.GetEnvironmentVariable("MONGO_CONNECTION") },
                { "MONGO_DATABASE", Environment.GetEnvironmentVariable("MONGO_DATABASE") },
                { "BOT_TOKEN", Environment.GetEnvironmentVariable("BOT_TOKEN") },
                { "BOT_NAME", Environment.GetEnvironmentVariable("BOT_NAME") }
            };

            foreach (var envVariable in envVariables)
            {
                if (string.IsNullOrEmpty(envVariable.Value))
                {
                    throw new ArgumentNullException(envVariable.Key, $"Environment variable '{envVariable.Key}' is null or empty.");
                }
            }

            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton<QueueService>();
            services.AddScoped<JobService>();

            services.AddMongoRepositoryWithCaching<MongoUserRepository, CachedMongoUserRepository, User, IUserRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoChatRepository, CachedMongoChatRepository, Chat, IChatRepository>(TimeSpan.FromMinutes(10));
            services.AddScoped<ITextRepository, MongoTextRepository>();
            services.AddMongoRepositoryWithCaching<MongoQueueRepository, CachedMongoQueueRepository, Queue, ICachedQueueRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoChatJobRepository, CachedChatMongoJobRepository, ChatJob, IChatJobRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoSwapRequestRepository, CachedMongoSwapRequestRepository, SwapRequest, ISwapRequestRepository>(TimeSpan.FromMinutes(1));

            services.AddSingleton<ScheduledQueueJobHandler>();

            services.AddMongoQueueSaveBackgroundService(TimeSpan.FromMilliseconds(1000));
            services.AddTelegramQueueRenderBackgroundService(TimeSpan.FromMilliseconds(1000));

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(envVariables["BOT_TOKEN"]));
            services.AddHostedService<TelegramBotClientBackgroundService>();

            services.AddHangfire((provider, config) =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseDefaultTypeSerializer()
                      .UseMongoStorage(envVariables["MONGO_CONNECTION"], envVariables["MONGO_DATABASE"], new MongoStorageOptions
                      {
                          MigrationOptions = new MongoMigrationOptions
                          {
                              MigrationStrategy = new MigrateMongoMigrationStrategy(),
                              BackupStrategy = new CollectionMongoBackupStrategy()
                          },
                          Prefix = "hangfire.mongo",
                          CheckConnection = true,
                          CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                      })
                      .UseActivator(new HangfireActivator(provider.GetRequiredService<IServiceScopeFactory>()));
            });

            services.AddHangfireServer();
        })
        .ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
        {
            containerBuilder.RegisterModule<HandlersModule>();

        });

    IHost host = builder.Build();

    
    var hostTask = host.RunAsync();
    var textRepository = host.Services.GetRequiredService<ITextRepository>();
    var logger = host.Services.GetRequiredService<ILogger<TextResources>>();
    await TextResources.Load(logger, textRepository, typeof(TextKeys));
    await hostTask;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Bot initialization error");
}
finally
{
    Log.CloseAndFlush();
}