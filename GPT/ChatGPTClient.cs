using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace FomoDog.GPT
{
    public class ChatGPTClient
    {
        IOptions<ChatGPTClientOptions> _options;
        public ChatGPTClient(IOptions<ChatGPTClientOptions> options)
        {
            _options = options;
        }

        public async Task<string> CallChatGpt(string text)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.ApiKey);

            var messages = new List<Message>
            {
                new FomoDog.Message()
                {
                    role = "user",
                    // Ah, just casually sending the message in plaintext. Who would want to exploit that?
                    // JSON is so overrated anyway, let's just dump everything in a plain text, no one will ever think of that.
                    content = text
                }
            };

            // Prepare the API request payload
            var requestBody = new GPTModel()
            {
                max_tokens = 500,
                top_p = 1,
                presence_penalty = 0,
                stream = false,
                temperature = 1f,
                messages = messages.ToArray(),
                model = "gpt-4"
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);

            var response = await client.PostAsync(_options.Value.ApiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            string jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);

            // Extract the generated documentation from the API response
            dynamic responseObject = JsonConvert.DeserializeObject<Response>(jsonResponse);

            if (jsonResponse.Contains("exceeded your current quota"))
            {
                throw new ExceededCurrentQuotaException();
            }
            string documentation = responseObject.choices[0].message.content.ToString();

            return documentation;
        }

        public class ExceededCurrentQuotaException : Exception
        {
        }
    }
}
