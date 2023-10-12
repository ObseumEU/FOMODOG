namespace FomoDog.Context.Database.Models
{
    public class Conversation
    {
        public string Id { get; set; } // Primary Key
        public List<Activity> Activities { get; set; }
    }
}
