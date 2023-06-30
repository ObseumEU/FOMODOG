namespace FomoDog.Context.Models
{
    public class ChatActivity
    {
        public string ChatId { get; set; }
        public string From { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string RawMessage { get; set; }

        public override string ToString()
        {
            return $"From:{From} Date:{Date.ToString("yyyy MMMM d hh:mm:ss")} Content:{Content}";
        }
    }
}