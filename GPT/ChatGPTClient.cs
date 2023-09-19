﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace FomoDog.GPT
{
    public class ChatGPTClient
    {
        readonly IOptions<ChatGPTClientOptions> _options;
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
                    Role = "user",
                    // Ah, just casually sending the message in plaintext. Who would want to exploit that?
                    // JSON is so overrated anyway, let's just dump everything in a plain text, no one will ever think of that.
                    Content = text
                }
            };

            // Prepare the API request payload
            var requestBody = new GptModel()
            {
                MaxTokens = 1500, //Hardcoded? What to expect when developing from a phone on the beach?
                TopP = 1,
                PresencePenalty = 0,
                Stream = false,
                Temperature= 1f,
                Messages = messages.ToArray(),
                Model = "gpt-4"
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);

            var response = await client.PostAsync(_options.Value.ApiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            string jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);

            // Extract the generated documentation from the API response
            Response responseObject = JsonConvert.DeserializeObject<Response>(jsonResponse);

            if (jsonResponse.Contains("exceeded your current quota"))
            {
                throw new ExceededCurrentQuotaException();
            }
            string documentation = responseObject.Choices[0].ResponseMessage.Content.ToString();

            return documentation;
        }

        public class ExceededCurrentQuotaException : Exception
        {
        }
    }
}
