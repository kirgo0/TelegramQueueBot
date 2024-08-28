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
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Context;
using TelegramQueueBot.Extensions;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Jobs;
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

            var mongoConnectionString = context.Configuration["MongoSettings:Connection"];
            var mongoDatabaseName = context.Configuration["MongoSettings:DatabaseName"];

            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton(provider =>
            {
                return new QueueService(provider.GetRequiredService<ICachedQueueRepository>());
            });

            services.AddMongoRepositoryWithCaching<MongoUserRepository, CachedMongoUserRepository, User, IUserRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoChatRepository, CachedMongoChatRepository, Chat, IChatRepository>(TimeSpan.FromMinutes(10));
            services.AddMongoRepositoryWithCaching<MongoTextRepository, CachedMongoTextRepository, Text, ITextRepository>(TimeSpan.FromMinutes(60));
            services.AddMongoRepositoryWithCaching<MongoQueueRepository, CachedMongoQueueRepository, Queue, ICachedQueueRepository>(TimeSpan.FromMinutes(10));
            services.AddScoped<IChatJobRepository, ChatJobMongoRepository>();

            services.AddMongoQueueSaveBackgroundService(TimeSpan.FromSeconds(1));
            services.AddTelegramQueueRenderBackgroundService(TimeSpan.FromMilliseconds(1000));

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(context.Configuration.GetSection("TelegramBotOptions")["Token"]));
            services.AddHostedService<TelegramBotClientBackgroundService>();

            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseDefaultTypeSerializer()
                      .UseMongoStorage(mongoConnectionString, mongoDatabaseName, new MongoStorageOptions
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
                      .UseActivator(new HangfireActivator(services.BuildServiceProvider()));
            });
            services.AddScoped<CreateScheduledQueueJob>();
            services.AddScoped<JobService>();

            services.AddHangfireServer();
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

