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
    }
}
