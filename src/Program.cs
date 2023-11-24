using FomoDog;
using FomoDog.Context;
using FomoDog.Context.MongoDB;
using FomoDog.Context.MongoDB.FomoDog.Context.MongoDB;
using FomoDog.GPT;
using FomoDog.OpenTelemetry;
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

        services.Configure<MongoDBOptions>(config.GetSection("MongoDBOptions"));

        var mongoDbOptions = config.GetSection("MongoDBOptions").Get<MongoDBOptions>();
        services.AddSingleton<IMongoDBIndexInitializer, MongoDBIndexInitializer>();
        services.AddScoped<IChatRepository, MongoDBRepository>();

        services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(mongoDbOptions.ConnectionString);
            });

        services.AddScoped<HttpClient>();
        services.AddScoped<MetadataDownloader>();

        services.AddScoped<IFileSystem, FileSystem>();

        services.Configure<ChatbotOptions>(config.GetSection("Chatbot"));
        services.AddScoped<Chatbot>();

        services.AddScoped<ITelegramBotClient>(serviceProvider =>
            {
                services.Configure<TelegramOptions>(config.GetSection("Telegram"));
                var telegramConfig = config.GetSection("Telegram").Get<TelegramOptions>();
                return new TelegramBotClient(telegramConfig.Key);
            });

        services.Configure<ChatGPTClientOptions>(config.GetSection("ChatGPT"));
        services.AddScoped<ChatGPTClient>();
    })
    .Build();

using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
var indexInitializer = provider.GetRequiredService<IMongoDBIndexInitializer>();
await indexInitializer.EnsureIndexesCreatedAsync();

Chatbot chatbot = provider.GetRequiredService<Chatbot>();
await chatbot.Run();
await host.RunAsync();
