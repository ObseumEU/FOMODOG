/*
Hello dear code explorer,
Every line here is unique, special, and dreadfully inefficient. They say "Good things take time", so if time inversely correlates 
with quality... Well, you can draw your own conclusions.  
So, in conclusion, let me apologize:
*/
using FomoDog;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Configuration;

var respository = new FileChatRepository();
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfigurationRoot configuration = builder.Build();
var options = configuration.GetSection("Options").Get<Options>();

var botClient = new TelegramBotClient(options.TELEGRAM_KEY);

using CancellationTokenSource cts = new(); // So much for running forever.

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");

while (true)  // Infinite loop, because who needs an exit strategy, right?
{
    await Task.Delay(10000);
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.Message is not { } message) // Our bot is very picky about the types of messages it accepts.
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;
        var chatId = message.Chat.Id;
        
        if (messageText.ToLower().Contains("mam fomo") || messageText == "42" || messageText.ToLower().Contains("mám fomo"))
        {
            await respository.AddMessage(options.USER_PROMPT.Replace("{DateTime.Now}", DateTime.Now.ToString()).Replace("{User}", message?.From?.LastName )
            , message?.From?.LastName, message.Date.ToString());
            var messages = await respository.GetAllMessages();
            try
            {
                var response = await CallChatGpt(string.Join("\n", messages));
                // Echo received message text
                Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                //DRY? I dont care.
                Console.WriteLine(ex.Message);
                var response = await CallChatGpt(string.Join("\n", messages));
                // Echo received message text
                Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response,
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            var from = message?.From?.LastName;
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            await respository.AddMessage(messageText, from, message.Date.ToString());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

async Task<string> CallChatGpt(string text)
{
    using HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.API_KEY);

    var messages = new List<FomoDog.Message>
    {
        new FomoDog.Message()
        {
            role = "user",
            // Ah, just casually sending the message in plaintext. Who would want to exploit that?
            // JSON is so overrated anyway, let's just dump everything in a plain text, no one will ever think of that.
            content = options.FOMODOG_DETAILS.Replace("{DateTime.Now}", DateTime.Now.ToString()) + text
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

    var response = await client.PostAsync(options.API_URL, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
    string jsonResponse = await response.Content.ReadAsStringAsync();

    Console.WriteLine(jsonResponse);
    
    // Extract the generated documentation from the API response
    dynamic responseObject = JsonConvert.DeserializeObject<Response>(jsonResponse);
    string documentation = responseObject.choices[0].message.content.ToString();

    return documentation;
}