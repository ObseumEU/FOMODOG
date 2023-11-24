using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace FomoDog.Tests
{
    public class MetadataDownloaderTests
    {
        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private MetadataDownloader? downloader;

        public MetadataDownloaderTests() => mockHttpClientFactory = new Mock<IHttpClientFactory>();

        private void SetupHttpClientHandler(string url, string contentType, string content)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(url).Respond(contentType, content);

            var httpClient = new HttpClient(mockHttp);
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            downloader = new MetadataDownloader(mockHttpClientFactory.Object);
        }

        [Theory]
        [InlineData("<html><head><title>Example Domain</title></head><body></body></html>", "Example Domain")]
        [InlineData("<html><head></head><body></body></html>", null)]
        [InlineData("", null)]
        [InlineData("<html><head><title>First Title</title><title>Second Title</title></head><body></body></html>", "First Title")]
        public async Task DownloadMetadata_ShouldHandleDifferentHtmlResponses(string htmlContent, string expectedTitle)
        {
            // Arrange
            SetupHttpClientHandler("http://example.com", "text/html", htmlContent);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            if (expectedTitle != null)
            {
                Assert.NotNull(metadata);
                Assert.Equal(expectedTitle, metadata.Title);
            }
            else
            {
                Assert.Null(metadata);
            }
        }

        [Fact]
        public async Task DownloadMetadata_ShouldHandleNetworkIssuesLikeTimeout()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://example.com").Throw(new TaskCanceledException()); // Simulating a timeout

            var httpClient = new HttpClient(mockHttp);
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            downloader = new MetadataDownloader(mockHttpClientFactory.Object);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.Null(metadata);
        }
    }

}
