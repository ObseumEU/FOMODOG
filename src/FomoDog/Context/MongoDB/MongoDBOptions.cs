namespace FomoDog.Context.MongoDB
{
    public class MongoDBOptions
    {
        public int MaxMessagesStoreCount { get; set; } = 90;
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }
}