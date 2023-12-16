namespace FomoDog.ChatGPT
{
    public interface IChatGPTClient
    {
        Task<string> CallChatGpt(string text);
    }
}