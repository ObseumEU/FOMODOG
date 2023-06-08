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
using FomoDog.GPT;

const string BOT_NAME = "FOMODOG";

var respository = new FileChatRepository();
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfigurationRoot configuration = builder.Build();
var options = configuration.GetSection("Options").Get<Options>();

var botClient = new TelegramBotClient(options.TELEGRAM_KEY);
var gpt = new ChatGPTClient(options.FOMODOG_DETAILS, options.API_KEY, options.API_URL);

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
            var prompt = ReplaceVariables(options.USER_PROMPT, message?.From?.LastName);
            await respository.AddMessage(prompt, message?.From?.LastName, GetDate());
            var messages = await respository.GetAllMessages();
            try
            {
                var response = await gpt.CallChatGpt(string.Join("\n", messages));
                // Echo received message text
                Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response,
                    cancellationToken: cancellationToken);
                await respository.AddMessage(response, BOT_NAME, GetDate());
            }
            catch (Exception ex)
            {
                //DRY? I dont care.
                Console.WriteLine(ex.Message);
                var response = await gpt.CallChatGpt(string.Join("\n", messages));
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
            await respository.AddMessage(messageText, from, GetDate());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

string GetDate()
{
    return DateTime.Now.ToString("yyyy MMMM d hh:mm:ss");
}

string ReplaceVariables(string text, string username)
{
    return text
        .Replace("{DateTime.Now}", GetDate())
        .Replace("{User}", username);
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