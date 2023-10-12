using FomoDog.Context.Models;

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
}
