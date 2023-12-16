using MassTransit;

namespace FomoDog.ChatGPT
{
    public class MessageConsumer : IConsumer<MessageTypes.GPT.GetChatGPTCompletion>
    {
        readonly ILogger<MessageConsumer> _logger;
        readonly IChatGPTClientFactory _chatGPTClientFactory;

        public MessageConsumer(ILogger<MessageConsumer> logger, IChatGPTClientFactory chatGPTClientFactory)
        {
            _logger = logger;
            _chatGPTClientFactory = chatGPTClientFactory;
        }

        public async Task Consume(ConsumeContext<MessageTypes.GPT.GetChatGPTCompletion> context)
        {
            var client = await _chatGPTClientFactory.CreateClientAsync();
            var result = await client.CallChatGpt(context.Message.Text);
            var message = new MessageTypes.GPT.ChatGPTCompletion()
            {
                Text = result
            };
            await context.RespondAsync(message);
        }
    }
}
