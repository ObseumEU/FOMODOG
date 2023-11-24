
namespace FomoDog
{
    public interface IMetadataDownloader
    {
        Task<Metadata> DownloadMetadata(string url);
    }
}