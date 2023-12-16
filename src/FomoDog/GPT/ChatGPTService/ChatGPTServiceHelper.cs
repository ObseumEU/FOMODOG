using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FomoDog.GPT.ChatGPTService
{
    public static class ChatGPTServiceHelper
    {
        public static IServiceCollection AddChatGPTService(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddScoped<ChatGPTServiceClient>();
            services.AddScoped<IChatGPTClientFactory, ChatGPTClientFactory>();
            return services;
        }
    }
}
