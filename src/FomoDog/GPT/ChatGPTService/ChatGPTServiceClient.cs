using MassTransit;

namespace FomoDog.GPT.ChatGPTService
{
    public class ChatGPTServiceClient : IChatGPTClient
    {
        IRequestClient<MessageTypes.GPT.GetChatGPTCompletion> _client;
        public ChatGPTServiceClient(IRequestClient<MessageTypes.GPT.GetChatGPTCompletion> client)
        {
            _client = client;
        }

        public async Task<string> CallChatGpt(string text)
        {
            var response = await _client.GetResponse<MessageTypes.GPT.ChatGPTCompletion>(new MessageTypes.GPT.GetChatGPTCompletion() 
            { 
                Text = text 
            });
            return response.Message.Text;
        }
    }
}
