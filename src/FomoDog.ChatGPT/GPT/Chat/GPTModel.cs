using Newtonsoft.Json;

namespace FomoDog.ChatGPT
{
    public class GptModel
    {
        [JsonProperty("messages")]
        public Message[] Messages { get; set; }

        [JsonProperty("temperature")]
        public float Temperature { get; set; }

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("top_p")]
        public float TopP { get; set; }

        [JsonProperty("frequency_penalty")]
        public float FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty")]
        public float PresencePenalty { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; }
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class Response
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("_object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("usage")]
        public ResponseUsage ResponseUsage { get; set; }

        [JsonProperty("choices")]
        public Choice[] Choices { get; set; }
    }

    public class ResponseUsage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class Choice
    {
        [JsonProperty("message")]
        public ResponseMessage ResponseMessage { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }

    public class ResponseMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}