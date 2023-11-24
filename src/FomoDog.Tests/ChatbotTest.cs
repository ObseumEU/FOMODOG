using FomoDog.Context;
using FomoDog.Context.Models;
using FomoDog.GPT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots.Types;
using Xunit;
using static FomoDog.GPT.ChatGPTClient;

namespace FomoDog.Tests
{
    public class ChatbotTest
    {
        [Fact]
        public async Task ReceiveMessage_ShouldProcessTextMessage()
        {
            // Arrange
            var mockGptClient = new Mock<IChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<IMetadataDownloader>();
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

        [Fact]
        public async Task ReceiveMessage_ShouldProcessMessageWithLink()
        {
            // Arrange
            var mockGptClient = new Mock<IChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<IMetadataDownloader>();
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
                Text = "Check out this link: https://example.com",
                From = new Telegram.Bot.Types.User { Id = 67890, FirstName = "Test", LastName = "User" },
                Date = DateTime.UtcNow
            };

            var metadata = new Metadata { Description = "Example Description" };
            mockMetadataDownloader.Setup(m => m.DownloadMetadata("https://example.com"))
                                  .ReturnsAsync(metadata);

            // Act
            await dialogFlow.ReceiveMessage(message, CancellationToken.None);

            // Assert
            // Verify that metadata is downloaded for the link
            mockMetadataDownloader.Verify(m => m.DownloadMetadata("https://example.com"), Times.Once);

            // Verify that the chat repository was called to add a new chat activity with updated content
            mockChatRepository.Verify(repo => repo.AddActivity(It.Is<ChatActivity>(activity =>
                activity.Content.Contains("https://example.com (Example Description)") &&
                activity.ChatId == "12345")), Times.Once);
        }

        [Fact]
        public async Task ReceiveMessage_ShouldLogErrorOnException()
        {
            // Arrange
            var mockGptClient = new Mock<IChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<IMetadataDownloader>();
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

            mockChatRepository.Setup(repo => repo.AddActivity(It.IsAny<ChatActivity>()))
                              .Throws(new InvalidOperationException("Error processing message"));

            // Act
            await dialogFlow.ReceiveMessage(message, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing message")),
                    It.IsAny<InvalidOperationException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task ReceiveMessage_ShouldLogRegularTextMessage()
        {
            // Arrange
            var mockGptClient = new Mock<IChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<IMetadataDownloader>();
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
                Text = "Hello, how are you?",
                From = new Telegram.Bot.Types.User { Id = 67890, FirstName = "Test", LastName = "User" },
                Date = DateTime.UtcNow
            };

            // Act
            await dialogFlow.ReceiveMessage(message, CancellationToken.None);

            // Assert
            // Verify that the chat repository was called to add a new chat activity with the message text
            mockChatRepository.Verify(repo => repo.AddActivity(It.Is<ChatActivity>(activity =>
                activity.Content == "Hello, how are you?" &&
                activity.ChatId == "12345")), Times.Once);
        }

        [Fact]
        public async Task ReceiveMessage_ShouldLogErrorOnExceededQuotaException()
        {
            // Arrange
            var mockGptClient = new Mock<IChatGPTClient>();
            var mockChatbotOptions = new Mock<IOptions<ChatbotOptions>>();
            var mockTelegramOptions = new Mock<IOptions<TelegramOptions>>();
            var mockLogger = new Mock<ILogger<DialogFlow>>();
            var mockChatRepository = new Mock<IChatRepository>();
            var mockMetadataDownloader = new Mock<IMetadataDownloader>();
            var mockTelegramBotClient = new Mock<ITelegramBotClient>();

            var options = new ChatbotOptions
            {
                UserPrompt = "Your user prompt here",
                ChatDetails = "SomeChatDetails"
            };
            mockChatbotOptions.Setup(o => o.Value).Returns(options);

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
                Text = "mam fomo",
                From = new Telegram.Bot.Types.User { Id = 67890, FirstName = "Test", LastName = "User" },
                Date = DateTime.UtcNow
            };

            mockGptClient.Setup(m => m.CallChatGpt(It.IsAny<string>()))
                         .ThrowsAsync(new ExceededCurrentQuotaException("Quota exceeded"));

            // Act
            await dialogFlow.ReceiveMessage(message, CancellationToken.None);

            // Assert
            // Verify that the logger logs the exception
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Quota exceeded")),
                    It.IsAny<ExceededCurrentQuotaException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        public class ExceededCurrentQuotaException : Exception
        {
            public ExceededCurrentQuotaException(string message) : base(message) { }
        }
    }

  
}
