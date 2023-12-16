using MassTransit;

namespace FomoDog.ChatGPT
{
    public class MessageConsumer : IConsumer<MessageTypes.GPT.GetChatGPTCompletion>
    {
        readonly ILogger<MessageConsumer> _logger;

        public MessageConsumer(ILogger<MessageConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<MessageTypes.GPT.GetChatGPTCompletion> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Text);
            var message = new MessageTypes.GPT.ChatGPTCompletion()
            {
                Text = "TODO GPT response text"
            };
            context.Respond(message);
            return Task.CompletedTask;
        }
    }
}
