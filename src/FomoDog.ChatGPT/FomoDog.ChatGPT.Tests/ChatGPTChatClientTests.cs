using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;
using FomoDog.ChatGPT;
using Moq.Language.Flow;

namespace FomoDog.ChatGPT.Tests
{
    public class ChatGPTChatClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly ChatGPTChatClient _client;
        private readonly Mock<IOptions<ChatGPTClientOptions>> _optionsMock;

        public ChatGPTChatClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _optionsMock = new Mock<IOptions<ChatGPTClientOptions>>();
            _optionsMock.Setup(_ => _.Value).Returns(new ChatGPTClientOptions
            {
                ApiKey = "test-api-key",
                ApiUrl = "https://api.example.com",
                MaxTokens = 100,
                Temperature = 0.7f,
                TopP = 0.9f,
                PresencePenalty = 0.5f,
                Model = "gpt-4"
            });

            var loggerMock = new Mock<ILogger<ChatGPTChatClient>>();
            _client = new ChatGPTChatClient(_optionsMock.Object, httpClientFactoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task CallChatGpt_ShouldReturnResponse_WhenApiCallIsSuccessful()
        {
            // Arrange
            var expectedResponse = new Response
            {
                Choices = new[]
                {
                    new Choice { ResponseMessage = new ResponseMessage { Content = "Test response" } }
                }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(httpResponse);

            // Act
            var result = await _client.CallChatGpt("Hello, GPT!");

            // Assert
            result.Should().Be(expectedResponse.Choices[0].ResponseMessage.Content);
        }

        [Fact]
        public async Task CallChatGpt_ShouldThrowException_WhenApiResponseIsError()
        {
            // Arrange
            _handlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.CallChatGpt("Hello, GPT!"));
        }

        // Additional tests like handling exceeded quota exception can be added here.
    }
}

