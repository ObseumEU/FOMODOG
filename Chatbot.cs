/*
Hello dear code explorer,
Every line here is unique, special, and dreadfully inefficient. They say "Good things take time", so if time inversely correlates 
with quality... Well, you can draw your own conclusions.  
So, in conclusion, let me apologize:
*/

using FomoDog.GPT;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bots.Types;

namespace FomoDog
{
    public class Chatbot
    {
        const string BOT_NAME = "FOMODOG";
        FileChatRepository _respository;
        ChatGPTClient _gpt;
        IOptions<ChatbotOptions> _chatbotOptions;
        IOptions<TelegramOptions> _telegramOptions;

        public Chatbot(IOptions<ChatbotOptions> chatbotOptions, FileChatRepository respository, ChatGPTClient gpt, IOptions<TelegramOptions> telegramOptions)
        {
            _chatbotOptions = chatbotOptions;
            _telegramOptions = telegramOptions;
            _respository = respository;
            _gpt = gpt;
        }

        public async Task Run()
        {
            var botClient = new TelegramBotClient(_telegramOptions.Value.Key);

            using CancellationTokenSource cts = new(); // So much for running forever.

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>() // receive all update types
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
        }

        public static string[] ExtractHttpsLinks(string inputText)
        {
            const string pattern = @"https://\S+";
            var matches = Regex.Matches(inputText, pattern);
            var links = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                links[i] = matches[i].Value;
            }

            return links;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is not { } message) // Our bot is very picky about the types of messages it accepts.
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;
                var chatId = message.Chat.Id;
                var from = $"{message?.From?.FirstName} {message?.From?.LastName}";
                if (messageText.ToLower().Contains("mam fomo") || messageText == "42" || messageText.ToLower().Contains("mám fomo"))
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                            text: "Moment, jen si projdu chat.",
                            cancellationToken: cancellationToken);

                    var prompt = ReplaceVariables(_chatbotOptions.Value.UserPrompt, from);
                    await _respository.AddMessage(prompt, from, GetDate());
                    var messages = await _respository.GetAllMessages();
                    try
                    {
                        var response = await _gpt.CallChatGpt(_chatbotOptions.Value.ChatDetails.Replace("{DateTime.Now}", DateTime.Now.ToString()) + string.Join("\n", messages));
                        // Echo received message text

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: response,
                            cancellationToken: cancellationToken);
                        await _respository.AddMessage(response, BOT_NAME, GetDate());
                    }
                    catch (Exception ex)
                    {
                        //DRY? I dont care.
                        Console.WriteLine(ex.Message);
                        var response = await _gpt.CallChatGpt(_chatbotOptions.Value.ChatDetails.Replace("{DateTime.Now}", DateTime.Now.ToString()) + string.Join("\n", messages));
                        // Echo received message text
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: response,
                            cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    Console.WriteLine($"Received from telefram '{JsonConvert.SerializeObject(message)}");
                    var links = ExtractHttpsLinks(messageText);
                    if (links?.Count() > 0)
                    {
                        foreach (var link in links)
                        {
                            var metadata = new MetadataDownloader().DownloadMetadata(link);
                            messageText = messageText.Replace(link, $"{link} ({metadata.Description})");
                        }
                    }

                    await _respository.AddMessage(messageText, from, GetDate());
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
    }
}
