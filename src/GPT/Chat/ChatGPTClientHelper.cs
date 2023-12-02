using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace FomoDog.GPT.Chat
{
    public static class ChatGPTClientHelper
    {
        public static IServiceCollection AddChatGPTChatClient(this IServiceCollection services, IConfigurationRoot config)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            services.AddHttpClient("Gpt")
                .AddPolicyHandler(retryPolicy);
            services.Configure<ChatGPTClientOptions>(config.GetSection("ChatGPT"));
            services.AddScoped<ChatGPTChatClient>();
            services.AddScoped<IChatGPTClientFactory, ChatGPTClientFactory>();
            return services;
        }
    }
}
