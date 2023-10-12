using FomoDog.Context.Database.Models;
using FomoDog.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace FomoDog.Context.Database
{
    public class DatabaseRepository : IChatRepository
    {
        private readonly ChatDbContext _dbContext;
        private ActivityMapper _mapper;

        public DatabaseRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
            _mapper = new ActivityMapper();
        }

        public async Task AddActivity(ChatActivity activity)
        {
            var conversation = await _dbContext.Conversations.FindAsync(activity.ChatId);
            if (conversation == null)
            {
                _dbContext.Conversations.Add(new Conversation()
                {
                    Id = activity.ChatId,
                    Activities = new List<Activity>()
                    {
                        _mapper.ChatActivityToActivity(activity)
                    }
                });
            }
            else
            {
                var newActivity = _mapper.ChatActivityToActivity(activity);
                newActivity.Conversation = conversation;
                await _dbContext.ChatActivities.AddAsync(newActivity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChatActivity>> GetAllActivity(string chatId)
        {
            return await _dbContext.ChatActivities
                .Where(c => c.Conversation.Id == chatId)
                .Select(c => _mapper.ActivityToChatActivity(c))
                .ToListAsync();
        }

        public async Task TrimActivity(string chatId, int maxMessagesStoreCount)
        {
            var activitiesToRemove = _dbContext.ChatActivities
                .Where(c => c.Conversation.Id == chatId)
                .OrderByDescending(c => c.Date)
                .Skip(maxMessagesStoreCount)
                .ToList();

            _dbContext.ChatActivities.RemoveRange(activitiesToRemove);
            await _dbContext.SaveChangesAsync();
        }
    }

}
