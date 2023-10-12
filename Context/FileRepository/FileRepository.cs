using FomoDog.Context.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FomoDog.Context.FileRepository
{
    public class FileRepository : IChatRepository
    {
        readonly IFileSystem _fileSystem;
        readonly IOptions<FileRepositoryOption> _options;
        public FileRepository(IFileSystem fileSystem, IOptions<FileRepositoryOption> options)
        {
            _fileSystem = fileSystem;
            _options = options;
        }

        private string GetConversationRepositoryPath(string chatId)
        {
            return _options.Value.RepositoryPath + chatId;
        }

        public async Task AddActivity(ChatActivity activity)
        {
            var activities = await GetAllActivity(activity.ChatId);
            activities.Add(activity);
            await Save(activities, activity.ChatId);

            if (activities.Count > _options.Value.MaxMessagesStoreCount)
                await TrimActivity(activity.ChatId, _options.Value.MaxMessagesStoreCount);
        }

        private Task Save(List<ChatActivity> activities, string chatId)
        {
            var serialized = JsonConvert.SerializeObject(activities, Formatting.Indented);
            _fileSystem.File.WriteAllText(GetConversationRepositoryPath(chatId), serialized);
            return Task.CompletedTask;
        }

        public async Task<List<ChatActivity>> GetAllActivity(string chatId)
        {
            if (!_fileSystem.File.Exists(GetConversationRepositoryPath(chatId)))
                await _fileSystem.File.WriteAllTextAsync(GetConversationRepositoryPath(chatId), "[]");

            var serialized = _fileSystem.File.ReadAllText(GetConversationRepositoryPath(chatId));
            var activities = JsonConvert.DeserializeObject<List<ChatActivity>>(serialized);
            return activities.Where(a => a.ChatId == chatId).ToList();
        }

        public async Task TrimActivity(string chatId, int maxMessagesStoreCount)
        {
            var activities = await GetAllActivity(chatId);
            activities.RemoveRange(0, activities.Count - maxMessagesStoreCount);
            await Save(activities, chatId);
        }
    }
}
