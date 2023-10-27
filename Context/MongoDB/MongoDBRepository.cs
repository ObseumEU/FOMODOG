using FomoDog.Context.Models;
using FomoDog.Context.MongoDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FomoDog.Context.MongoDB
{
    public class MongoDBRepository : IChatRepository
    {
        private ActivityMapper _mapper;
        private IOptions<MongoDBOptions> _options;
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<Activity> _activities;

        public MongoDBRepository(IOptions<MongoDBOptions> options)
        {
            _mapper = new ActivityMapper();
            _options = options;
            _client = new MongoClient(options.Value.ConnectionString);
            _database = _client.GetDatabase("Fomodog");
            _activities = _database.GetCollection<Activity>("activities");
        }

        public async Task AddActivity(ChatActivity activity)
        {
            var newRecord = _mapper.ChatActivityToActivity(activity);
            await _activities.InsertOneAsync(newRecord);
        }

        public async Task<List<ChatActivity>> GetAllActivity(string chatId)
        { 
            // Build the filter
            var filter = Builders<Activity>.Filter.Eq("chat", chatId);
            // Sort (Descending by date - assuming you want the latest activities)
            var sort = Builders<Activity>.Sort.Descending("date"); //
            // Executing the query
            var activities = await _activities.Find(filter)
                                       .Sort(sort)
                                       .Limit(50) // take only the latest 50
                                       .ToListAsync();

            // Mapping the data
            var mappedActivities = activities.Select(a => _mapper.ActivityToChatActivity(a)).ToList();
            return mappedActivities;
        }

        public async Task TrimActivity(string chatId, int maxMessagesStoreCount)
        {
            
        }
    }
}
