namespace FomoDog.Context
{
    public class ChatRepositoryOption
    {
        public string RepositoryPath { get; set; } = "./data.txt";
        public int MaxMessagesStoreCount { get; set; } = 20;
    }
}
