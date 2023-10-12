using FomoDog.Context.Models;
using Riok.Mapperly.Abstractions;

namespace FomoDog.Context.Database.Models
{
    public class Activity
    {
        public int Id { get; set; } // Primary Key
        public string From { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string RawMessage { get; set; }
        public Conversation Conversation { get; set; }

        public override string ToString()
        {
            return $"From:{From} Date:{Date.ToString("yyyy MMMM d hh:mm:ss")} Content:{Content}";
        }
    }

    [Mapper]
    public partial class ActivityMapper
    {
        public partial Activity ChatActivityToActivity(ChatActivity activity);
        public partial ChatActivity ActivityToChatActivity(Activity activity);
    }
}
