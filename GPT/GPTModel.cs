namespace FomoDog
{

    public class GPTModel
    {
        public Message[] messages { get; set; }
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public int top_p { get; set; }
        public float frequency_penalty { get; set; }
        public int presence_penalty { get; set; }
        public string model { get; set; }
        public bool stream { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }



    public class Response
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public ResponsUsage usage { get; set; }
        public Choice[] choices { get; set; }
    }

    public class ResponsUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public ResponseMessage message { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
    }

    public class ResponseMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }


}
