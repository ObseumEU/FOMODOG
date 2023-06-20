using FomoDog;
using FomoDog.GPT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<Options>(config.GetSection("Options"));
        services.Configure<ChatGPTClientOptions>(config.GetSection("ChatGPT"));
        services.AddSingleton<ChatGPTClient>();
        services.AddSingleton<Chatbot>();
        services.AddSingleton<FileChatRepository>();
    })
    .Build();

using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
Chatbot chatbot = provider.GetRequiredService<Chatbot>();
await chatbot.Run();
await host.RunAsync();
