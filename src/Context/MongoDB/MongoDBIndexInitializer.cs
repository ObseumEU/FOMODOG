using FomoDog.Context.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FomoDog.Context.MongoDB
{
    namespace FomoDog.Context.MongoDB
    {
        public interface IMongoDBIndexInitializer
        {
            Task EnsureIndexesCreatedAsync();
        }

        public class MongoDBIndexInitializer : IMongoDBIndexInitializer
        {
            private readonly IMongoClient _mongoClient;
            private readonly IOptions<MongoDBOptions> _mongoDBOptions;

            public MongoDBIndexInitializer(IMongoClient mongoClient, IOptions<MongoDBOptions> mongoDBOptions)
            {
                _mongoClient = mongoClient;
                _mongoDBOptions = mongoDBOptions;
            }

            public async Task EnsureIndexesCreatedAsync()
            {
                var database = _mongoClient.GetDatabase(_mongoDBOptions.Value.Database);
                await database.CreateCollectionAsync("Activities");
                var collection = database.GetCollection<Activity>("Activities"); // Assuming your collection name is "Activities"

                // Create an example index on the ChatId field
                var chatIdIndex = new CreateIndexModel<Activity>(
                    Builders<Activity>.IndexKeys.Ascending(a => a.ChatId)
                );

                await collection.Indexes.CreateOneAsync(chatIdIndex);
            }
        }
    }

}
