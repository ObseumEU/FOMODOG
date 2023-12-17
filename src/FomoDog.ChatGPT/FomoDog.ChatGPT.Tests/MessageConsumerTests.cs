using MassTransit;
using Moq;
using Xunit;

namespace FomoDog.ChatGPT.Tests
{
    public class MessageConsumerTests
    {
        private readonly Mock<IChatGPTClientFactory> _chatGPTClientFactoryMock;
        private readonly Mock<IChatGPTClient> _chatGPTClientMock;
        private readonly Mock<ConsumeContext<MessageTypes.GPT.GetChatGPTCompletion>> _consumeContextMock;

        public MessageConsumerTests()
        {
            _chatGPTClientFactoryMock = new Mock<IChatGPTClientFactory>();
            _chatGPTClientMock = new Mock<IChatGPTClient>();
            _consumeContextMock = new Mock<ConsumeContext<MessageTypes.GPT.GetChatGPTCompletion>>();
        }

        [Fact]
        public async Task Consume_ValidMessage_CallsChatGPTClientAndResponds()
        {
            // Arrange
            var expectedResponse = "GPT Response";
            _chatGPTClientFactoryMock.Setup(f => f.CreateClientAsync()).ReturnsAsync(_chatGPTClientMock.Object);
            _chatGPTClientMock.Setup(f => f.CallChatGpt(It.IsAny<string>())).ReturnsAsync(expectedResponse);

            var testMessage = new MessageTypes.GPT.GetChatGPTCompletion { Text = "Test Message" };
            _consumeContextMock.Setup(m => m.Message).Returns(testMessage);

            var consumer = new MessageConsumer(_chatGPTClientFactoryMock.Object);

            // Act
            await consumer.Consume(_consumeContextMock.Object);

            // Assert
            _chatGPTClientMock.Verify(f => f.CallChatGpt(It.IsAny<string>()), Times.Once);
            _consumeContextMock.Verify(m => m.RespondAsync(It.Is<MessageTypes.GPT.ChatGPTCompletion>(msg => msg.Text == expectedResponse)), Times.Once);
        }
    }
}
