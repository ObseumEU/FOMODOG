/*
Hello dear code explorer,
Every line here is unique, special, and dreadfully inefficient. They say "Good things take time", so if time inversely correlates 
with quality... Well, you can draw your own conclusions.  
So, in conclusion, let me apologize:
*/
using FomoDog;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Configuration;
using FomoDog.GPT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Chatbot>();
    })
    .Build();


using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
Chatbot chatbot = provider.GetRequiredService<Chatbot>();
await chatbot.Run();
await host.RunAsync();
