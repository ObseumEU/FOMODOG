namespace FomoDog.Context.MongoDB
{
    public class MongoDBOptions
    {
        public int MaxMessagesStoreCount { get; set; } = 50;
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }
}