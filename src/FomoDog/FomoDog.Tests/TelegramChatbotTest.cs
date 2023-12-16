using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Xunit;
using Telegram.Bot.Types;


namespace FomoDog.Tests
{
    public class TelegramChatbotTest
    {
        [Fact]
        public async Task HandleUpdateAsync_ShouldProcessTextMessage()
        {
            // Arrange
            var mockBotClient = new Mock<ITelegramBotClient>();
            var mockLogger = new Mock<ILogger<TelegramChatbot>>();
            var mockDialogFlow = new Mock<IDialogFlow>();

            var services = new ServiceCollection();
            services.AddScoped<IDialogFlow>(_ => mockDialogFlow.Object);

            var serviceProvider = services.BuildServiceProvider();
            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(x => x.ServiceProvider).Returns(serviceProvider);
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                               .Returns(mockScopeFactory.Object);

            var telegramChatbot = new TelegramChatbot(mockBotClient.Object, mockServiceProvider.Object, mockLogger.Object);

            var message = new Telegram.Bot.Types.Message
            {
                Text = "Test message",
                Chat = new Chat { Id = 12345 }
            };
            var update = new Update { Message = message };

            // Act
            await telegramChatbot.HandleUpdateAsync(mockBotClient.Object, update, CancellationToken.None);

            // Assert
            mockDialogFlow.Verify(df => df.ReceiveMessage(It.Is<Telegram.Bot.Types.Message>(m => m.Text == "Test message" && m.Chat.Id == 12345), CancellationToken.None), Times.Once);
            mockLogger.VerifyNoOtherCalls(); // Verifies that no errors were logged
        }
    }
}
