namespace FomoDog.Context.FileRepository
{
    public class FileRepositoryOption
    {
        public string RepositoryPath { get; set; } = "./data.txt";
        public int MaxMessagesStoreCount { get; set; } = 20;
    }
}
