using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FomoDog.GPT
{
    public class ChatGPTClient
    {
        public ChatGPTClient() { }

        private readonly IOptions<ChatGPTClientOptions> _options;
        public ChatGPTClient(IOptions<ChatGPTClientOptions> options)
        {
            _options = options;
        }

        public async Task<string> CallChatGpt(string text)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.Value.ApiKey);

            var messages = new List<Message>
            {
                new Message
                {
                    Role = "user",
                    Content = text
                }
            };

            var requestBody = new GptModel
            {
                MaxTokens = _options.Value.MaxTokens,
                TopP = _options.Value.TopP,
                PresencePenalty = _options.Value.PresencePenalty,
                Stream = false,
                Temperature = _options.Value.Temperature,
                Messages = messages.ToArray(),
                Model = "gpt-4"
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);

            var response = await client.PostAsync(_options.Value.ApiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);

            var responseObject = JsonConvert.DeserializeObject<Response>(jsonResponse);

            if (jsonResponse.Contains("exceeded your current quota"))
            {
                throw new ExceededCurrentQuotaException();
            }
            var documentation = responseObject.Choices[0].ResponseMessage.Content.ToString();

            return documentation;
        }

        public class ExceededCurrentQuotaException : Exception
        {
        }
    }
}