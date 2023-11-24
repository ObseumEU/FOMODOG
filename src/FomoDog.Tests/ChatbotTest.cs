using FomoDog.Context;
using FomoDog.Context.Models;
using FomoDog.GPT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Telegram.Bot;
using Xunit;

namespace FomoDog.Tests
{
    public class ChatbotTest
    {
        [Fact]
        public async Task ReceiveMessage_ShouldProcessTextMessage()
        {
            // Arrange
            var mockGptClient = new Mock<ChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<MetadataDownloader>();
            var mockTelegramBotClient = new Mock<ITelegramBotClient>();

            var dialogFlow = new DialogFlow(
                mockChatbotOptions.Object,
                mockGptClient.Object,
                mockTelegramOptions.Object,
                mockLogger.Object,
                mockChatRepository.Object,
                mockMetadataDownloader.Object,
                mockTelegramBotClient.Object);

            var message = new Telegram.Bot.Types.Message
            {
                Chat = new Telegram.Bot.Types.Chat { Id = 12345 },
                Text = "Test message",
                From = new Telegram.Bot.Types.User { Id = 67890, FirstName = "Test", LastName = "User" },
                Date = DateTime.UtcNow
            };

            // Act
            await dialogFlow.ReceiveMessage(message, CancellationToken.None);

            // Assert
            // Verify that the chat repository was called to add a new chat activity
            mockChatRepository.Verify(repo => repo.AddActivity(It.Is<ChatActivity>(activity =>
                activity.Content.Contains("Test message") && activity.ChatId == "12345")), Times.Once);
        }
    }
}
