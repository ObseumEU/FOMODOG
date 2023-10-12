using FomoDog.Context.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FomoDog.Context.Database
{
    public class ChatDbContext : DbContext
    {

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Activity> ChatActivities { get; set; }
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
    }
}
