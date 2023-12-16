namespace FomoDog.GPT
{
    public interface IChatGPTClient
    {
        Task<string> CallChatGpt(string text);
    }
}