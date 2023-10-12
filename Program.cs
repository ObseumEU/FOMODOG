using FomoDog;
using FomoDog.Context;
using FomoDog.GPT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using System.IO.Abstractions;

IHostEnvironment env = Host.CreateDefaultBuilder(args).Build().Services.GetRequiredService<IHostEnvironment>();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, true)
    .AddEnvironmentVariables()
    .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddFeatureManagement(config);

        services.Configure<ChatRepositoryOption>(config.GetSection("Repository"));
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IChatRepository,ChatRepository>();

        services.Configure<ChatbotOptions>(config.GetSection("Chatbot"));
        services.AddScoped<Chatbot>();

        services.Configure<TelegramOptions>(config.GetSection("Telegram"));

        services.Configure<ChatGPTClientOptions>(config.GetSection("ChatGPT"));
        services.AddScoped<ChatGPTClient>();
    })
    .Build();

using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
Chatbot chatbot = provider.GetRequiredService<Chatbot>();
await chatbot.Run();
await host.RunAsync();
