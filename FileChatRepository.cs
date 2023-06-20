using Newtonsoft.Json;

namespace FomoDog
{
    public interface IChatRepository
    {
        Task AddMessage(string messageText, string from, string date);
        Task<List<string>> GetAllMessages();
    }

    /// <summary>
    /// Hold onto your hats, ladies and gents! Welcome to the "I'm-feeling-lazy-today" grand solution for storing messages!
    /// Embrace the wisdom of reading and writing from a file for each single message. Disk IO? Never heard of her.
    /// </summary>
    public class FileChatRepository : IChatRepository
    {
        private int _maxMessagesStoreCount;
        private string _filePath;

        public FileChatRepository(string filePath = "./messages.txt", int maxMessagesStoreCount = 30)
        {
            _maxMessagesStoreCount = maxMessagesStoreCount;
            _filePath = filePath;
        }

        public async Task AddMessage(string messageText, string from, string date)
        {
            // Because there's nothing more fun than re-reading a file each time a message arrives.
            // Hope you don't like performance.
            List<string>? messages = new List<string>();
            if (System.IO.File.Exists(_filePath))
                messages = JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText(_filePath)) ?? new List<string>();

            messages.Add($"Napsal uživatel:{from} v {date}: {messageText}");

            // Also, we'll just mercilessly delete the oldest message. Historical data is overrated anyway.
            if (messages.Count > _maxMessagesStoreCount)
                messages.RemoveAt(0);

            var serialized = JsonConvert.SerializeObject(messages, Formatting.Indented);

            // Look ma, I'm blocking the thread with synchronous file I/O. Ain't that a beauty?
            System.IO.File.WriteAllText(_filePath, serialized);
        }

        public async Task<List<string>> GetAllMessages()
        {
            var messages =  JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText(_filePath));
            return messages ?? new List<string>();
        }
    }
}
