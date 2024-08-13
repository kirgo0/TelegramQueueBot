﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueCore;
using QueueCore.BackgroundServices;
using QueueCore.Extensions;
using QueueCore.Repository.Interfaces;
using Serilog;
using Telegram.Bot;
using TelegramQueueBot;
using Microsoft.Extensions.Logging;
using Autofac;
using TelegramQueueBot.Modules;
using Autofac.Extensions.DependencyInjection;

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
            logging.AddSerilog(dispose: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddMemoryCache();

            services.AddMongoQueueRepository(opt =>
            {
                opt.ConnectionString = "mongodb://localhost:27017";
                opt.DatabaseName = "queue_bot_db";
                opt.CollectionName = "queues";
            });
            services.AddCachedMongoQueueRepository();
            services.AddSingleton<IQueueService, QueueService>(provider =>
            {
                return new QueueService(provider.GetRequiredService<ICachedQueueRepository>());
            });

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(context.Configuration.GetSection("TelegramBotOptions")["Token"]));

            services.AddHostedService<QueueSaveBackgroundService>();
            services.AddQueueRenderBackgroundService(TimeSpan.FromSeconds(1), (queue) =>
            {
                Console.WriteLine(queue.ToString());
            });
            services.AddHostedService<TelegramBotClientBackgroundService>();
        })
        .ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
        {
            containerBuilder.RegisterModule<CommandsModule>();
            containerBuilder.RegisterModule<HandlersModule>();
        });

    using IHost host = builder.Build();
    await host.RunAsync();

} catch(Exception ex)
{
    Log.Fatal("Bot initialization exception: {message}", ex.Message);
    throw;
}
finally
{
    Log.CloseAndFlush();
}
