using FomoDog.GPT.Chat;
using FomoDog.GPT.ChatGPTService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace FomoDog.GPT
{
    public interface IChatGPTClientFactory
    {
        Task<IChatGPTClient> CreateClientAsync();
    }

    public class ChatGPTClientFactory : IChatGPTClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFeatureManager _featureManager;

        public ChatGPTClientFactory(IServiceProvider serviceProvider, IFeatureManager featureManager)
        {
            _serviceProvider = serviceProvider;
            _featureManager = featureManager;
        }

        public async Task<IChatGPTClient> CreateClientAsync()
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.OPENAI_ASSISTANT_API))
            {
                throw new NotImplementedException();
            }
            else if (await _featureManager.IsEnabledAsync(FeatureFlags.MICROSERVICE_CHATGPT))
            {
                return _serviceProvider.GetRequiredService<ChatGPTServiceClient>();
            }
            else
            {
                return _serviceProvider.GetRequiredService<ChatGPTChatClient>();
            }
        }
    }
}
