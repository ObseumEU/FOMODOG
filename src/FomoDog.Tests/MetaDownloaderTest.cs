using RichardSzalay.MockHttp;
using Xunit;

namespace FomoDog.Tests
{
    public class MetadataDownloaderTests
    {
        [Fact]
        public async Task DownloadMetadata_ShouldExtractTitle_FromHtmlResponse()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var exampleHtml = "<html><head><title>Example Domain</title></head><body></body></html>";
            mockHttp.When("http://example.com").Respond("text/html", exampleHtml);

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal("Example Domain", metadata.Title);
        }

        [Fact]
        public async Task DownloadMetadata_ShouldReturnNull_IfTitleTagIsMissing()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var htmlWithoutTitle = "<html><head></head><body></body></html>";
            mockHttp.When("http://example.com").Respond("text/html", htmlWithoutTitle);

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.Null(metadata);
        }

        [Fact]
        public async Task DownloadMetadata_ShouldHandleNonHtmlResponse()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://example.com").Respond("text/plain", "Non-HTML Content");

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.Null(metadata);
        }

        [Fact]
        public async Task DownloadMetadata_ShouldReturnNull_ForEmptyHtmlContent()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://example.com").Respond("text/html", "");

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.Null(metadata);
        }

        [Fact]
        public async Task DownloadMetadata_ShouldExtractFirstTitle_FromHtmlWithMultipleTitles()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var htmlWithMultipleTitles = "<html><head><title>First Title</title><title>Second Title</title></head><body></body></html>";
            mockHttp.When("http://example.com").Respond("text/html", htmlWithMultipleTitles);

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal("First Title", metadata.Title); // Assuming it extracts the first title
        }

        [Fact]
        public async Task DownloadMetadata_ShouldHandleNetworkIssuesLikeTimeout()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://example.com").Throw(new TaskCanceledException()); // Simulating a timeout

            var httpClient = new HttpClient(mockHttp);
            var downloader = new MetadataDownloader(httpClient);

            // Act
            var metadata = await downloader.DownloadMetadata("http://example.com");

            // Assert
            Assert.Null(metadata);
        }
    }
}
