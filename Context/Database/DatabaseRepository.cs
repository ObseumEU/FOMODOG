using FomoDog.Context.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FomoDog.Context.Database
{

    public class DatabaseRepository : IChatRepository
    {
        readonly IOptions<DatabaseRepositoryOptions> _options;
        public DatabaseRepository(IOptions<DatabaseRepositoryOptions> options)
        {
            _options = options;
        }

        public async Task AddActivity(ChatActivity activity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatActivity>> GetAllActivity(string chatId)
        {
            throw new NotImplementedException();
        }

        public async Task TrimActivity(string chatId, int maxMessagesStoreCount)
        {
            throw new NotImplementedException();
        }
    }
}
