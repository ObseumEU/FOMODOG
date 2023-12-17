using FomoDog;
using FomoDog.Context;
using FomoDog.Context.MongoDB;
using FomoDog.Context.MongoDB.FomoDog.Context.MongoDB;
using FomoDog.GPT.Chat;
using FomoDog.GPT.ChatGPTService;
using FomoDog.OpenTelemetry;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using MongoDB.Driver;
using System.IO.Abstractions;
using Telegram.Bot;

IHostEnvironment env = Host.CreateDefaultBuilder(args).Build().Services.GetRequiredService<IHostEnvironment>();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, true)
    .AddEnvironmentVariables()
    .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((services) =>
    {
        services.AddConsoleOpenTelemetry(config.GetSection("OpenTelemetryOptions"));

        services.AddFeatureManagement(config);

        var featureManager = services.BuildServiceProvider().GetService<IFeatureManagerSnapshot>();
        bool isChatGPTServiceEnabled = featureManager.IsEnabledAsync(FeatureFlags.MICROSERVICE_CHATGPT).GetAwaiter().GetResult();

        if (isChatGPTServiceEnabled)
        {
            services.AddHttpClient();
            services.AddChatGPTService(config);
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMQConfig = context.GetRequiredService<IConfiguration>().GetSection("RabbitMQ");
                    cfg.Host(rabbitMQConfig["Host"], rabbitMQConfig["VirtualHost"], h =>
                    {
                        h.Username(rabbitMQConfig["Username"]);
                        h.Password(rabbitMQConfig["Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
        else
        {
            services.AddChatGPTChatClient(config);
        }

        services.Configure<MongoDBOptions>(config.GetSection("MongoDBOptions"));

        var mongoDbOptions = config.GetSection("MongoDBOptions").Get<MongoDBOptions>();
        services.AddSingleton<IMongoDBIndexInitializer, MongoDBIndexInitializer>();
        services.AddScoped<IChatRepository, MongoDBRepository>();

        services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(mongoDbOptions.ConnectionString);
            });

        services.AddScoped<IMetadataDownloader, MetadataDownloader>();

        services.AddScoped<IFileSystem, FileSystem>();

        services.Configure<ChatbotOptions>(config.GetSection("Chatbot"));
        services.AddScoped<TelegramChatbot>();

        services.AddScoped<ITelegramBotClient>(serviceProvider =>
            {
                services.Configure<TelegramOptions>(config.GetSection("Telegram"));
                var telegramConfig = config.GetSection("Telegram").Get<TelegramOptions>();
                return new TelegramBotClient(telegramConfig.Key);
            });
        services.AddScoped<IDialogFlow, DialogFlow>();


    })
    .Build();

using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
var indexInitializer = provider.GetRequiredService<IMongoDBIndexInitializer>();
await indexInitializer.EnsureIndexesCreatedAsync();

TelegramChatbot chatbot = provider.GetRequiredService<TelegramChatbot>();
await chatbot.Run();
await host.RunAsync();
