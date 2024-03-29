﻿using FomoDog.Context.Models;
using FomoDog.Context.MongoDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FomoDog.Context.MongoDB
{
    public class MongoDBRepository : IChatRepository
    {
        private readonly ActivityMapper _mapper;
        private readonly IMongoCollection<Activity> _activities;
        private readonly IOptions<MongoDBOptions> _options;

        // Constructor now takes a MongoClient and a database name. These can be mocked for testing.
        public MongoDBRepository(IMongoClient mongoClient, IOptions<MongoDBOptions> options)
        {
            if (mongoClient == null)
            {
                throw new ArgumentNullException(nameof(mongoClient));
            }

            _mapper = new ActivityMapper();
            var database = mongoClient.GetDatabase(options.Value.Database);
            _activities = database.GetCollection<Activity>("activities");
            _options = options;
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
                                       .Limit(_options.Value.MaxMessagesStoreCount)
                                       .ToListAsync();

            // Mapping the data
            var mappedActivities = activities.Select(a => _mapper.ActivityToChatActivity(a)).ToList();
            return mappedActivities;
        }
    }
}
