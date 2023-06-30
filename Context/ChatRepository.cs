using FomoDog.Context.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FomoDog.Context
{
    public interface IChatRepository
    {
        Task AddActivity(ChatActivity activity);

        /// <summary>
        /// Get latest messages
        /// </summary>
        Task<List<ChatActivity>> GetAllActivity(string chatId);

        /// <summary>
        /// Remove all messages except the last <paramref name="maxMessagesStoreCount"/> messages.
        /// </summary>
        Task TrimActivity(string chatId, int maxMessagesStoreCount);
    }

    public class ChatRepository : IChatRepository
    {
        readonly IFileSystem _fileSystem;
        readonly IOptions<ChatRepositoryOption> _options;
        public ChatRepository(IFileSystem fileSystem, IOptions<ChatRepositoryOption> options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }

        public async Task AddActivity(ChatActivity activity)
        {
            var activities = await GetAllActivity(activity.ChatId);
            activities.Add(activity);
            await Save(activities);

            if (activities.Count > _options.Value.MaxMessagesStoreCount)
                await TrimActivity(activity.ChatId, _options.Value.MaxMessagesStoreCount);
        }

        private async Task Save(List<ChatActivity> activities)
        {
            var serialized = JsonConvert.SerializeObject(activities, Formatting.Indented);
            _fileSystem.File.WriteAllText(_options.Value.RepositoryPath, serialized);
        }

        public async Task<List<ChatActivity>> GetAllActivity(string chatId)
        {
            if (!_fileSystem.File.Exists(_options.Value.RepositoryPath))
                _fileSystem.File.WriteAllText(_options.Value.RepositoryPath, "[]");

            var serialized = _fileSystem.File.ReadAllText(_options.Value.RepositoryPath);
            var activities = JsonConvert.DeserializeObject<List<ChatActivity>>(serialized);
            return activities;
        }

        public async Task TrimActivity(string chatId, int maxMessagesStoreCount)
        {
            var activities = await GetAllActivity(chatId);
            activities.RemoveRange(0, activities.Count - maxMessagesStoreCount);
            await Save(activities);
        }
    }
}
