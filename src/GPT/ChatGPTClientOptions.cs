namespace FomoDog.GPT
{
    public class ChatGPTClientOptions
    {
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public float TopP { get; set; }
        public float PresencePenalty { get; set; }
        public string Model { get; set; }
    }
}
