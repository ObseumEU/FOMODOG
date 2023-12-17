using FomoDog.GPT.ChatGPTService;
using Microsoft.Extensions.DependencyInjection;

namespace FomoDog.GPT
{
    public interface IChatGPTClientFactory
    {
        Task<IChatGPTClient> CreateClientAsync();
    }

    public class ChatGPTClientFactory : IChatGPTClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatGPTClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IChatGPTClient> CreateClientAsync()
        {
            return _serviceProvider.GetRequiredService<ChatGPTServiceClient>();
        }
    }
}
