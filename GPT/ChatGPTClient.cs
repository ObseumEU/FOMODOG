using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FomoDog.GPT
{
    public class ChatGPTClient
    {
        string _chatDetails;
        string _apiKey;
        string _apiUrl;
        public ChatGPTClient(string chatDetails, string apiKey, string apiUrl)
        {
            _chatDetails = chatDetails;
            _apiKey = apiKey;
            _apiUrl = apiUrl;
        }

        public async Task<string> CallChatGpt(string text)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var messages = new List<FomoDog.Message>
            {
                new FomoDog.Message()
                {
                    role = "user",
                    // Ah, just casually sending the message in plaintext. Who would want to exploit that?
                    // JSON is so overrated anyway, let's just dump everything in a plain text, no one will ever think of that.
                    content =_chatDetails.Replace("{DateTime.Now}", DateTime.Now.ToString()) + text
                }
            };

            // Prepare the API request payload
            var requestBody = new GPTModel()
            {
                max_tokens = 200,
                top_p = 1,
                presence_penalty = 0,
                stream = false,
                temperature = 0.5f,
                messages = messages.ToArray(),
                model = "gpt-4"
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);

            var response = await client.PostAsync(_apiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            string jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);

            // Extract the generated documentation from the API response
            dynamic responseObject = JsonConvert.DeserializeObject<Response>(jsonResponse);
            string documentation = responseObject.choices[0].message.content.ToString();

            return documentation;
        }
    }
}
