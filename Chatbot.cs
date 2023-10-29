/*
Hello dear code explorer,
Every line here is unique, special, and dreadfully inefficient. They say "Good things take time", so if time inversely correlates 
with quality... Well, you can draw your own conclusions.  
So, in conclusion, let me apologize:
*/

using FomoDog.Context;
using FomoDog.GPT;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using static FomoDog.GPT.ChatGPTClient;

namespace FomoDog
{
    public class Chatbot
    {
        const string BOT_NAME = "FOMODOG";
        readonly ChatGPTClient _gpt;
        readonly IOptions<ChatbotOptions> _chatbotOptions;
        readonly IOptions<TelegramOptions> _telegramOptions;
        readonly IChatRepositoryFactory _chatRepositoryFactory;
        IChatRepository _chatRepository;

        public Chatbot(IOptions<ChatbotOptions> chatbotOptions, ChatGPTClient gpt, IOptions<TelegramOptions> telegramOptions, IChatRepositoryFactory chatRepositoryFactory)
        {
            _chatbotOptions = chatbotOptions;
            _telegramOptions = telegramOptions;
            _gpt = gpt;
            _chatRepositoryFactory = chatRepositoryFactory;
        }

        public async Task Run()
        {
            ITelegramBotClient botClient = new TelegramBotClient(_telegramOptions.Value.Key);
            _chatRepository = await _chatRepositoryFactory.CreateRepositoryAsync();
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

                    var userPrompt = ReplaceVariables(_chatbotOptions.Value.UserPrompt);
                    await _chatRepository.AddActivity(new Context.Models.ChatActivity()
                    {
                        ChatId = message.Chat.Id.ToString(),
                        Content = userPrompt,
                        Date = message.Date,
                        From = message.From.FirstName + message.From.LastName,
                        RawMessage = JsonConvert.SerializeObject(message)
                    });
                    var messages = await _chatRepository.GetAllActivity(message.Chat.Id.ToString());

                    try
                    {
                        var activitiesTexts = messages.Select(m => m.ToString()).Select(ReplaceVariables).ToList();
                        var gptPrompt = _chatbotOptions.Value.ChatDetails.Replace("{DateTime.Now}", GetDateString()) + String.Join("\n", activitiesTexts);
                        var response = await _gpt.CallChatGpt(gptPrompt);
                        // Echo received message text

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: response,
                            cancellationToken: cancellationToken);
                        await _chatRepository.AddActivity(new Context.Models.ChatActivity()
                        {
                            ChatId = message.Chat.Id.ToString(),
                            Content = response,
                            Date = message.Date,
                            From = BOT_NAME,
                            RawMessage = JsonConvert.SerializeObject(message)
                        });
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
                            var metadata = await new MetadataDownloader().DownloadMetadata(link);
                            messageText = messageText.Replace(link, $"{link} ({metadata.Description})");
                        }
                    }

                    await _chatRepository.AddActivity(new Context.Models.ChatActivity()
                    {
                        ChatId = message.Chat.Id.ToString(),
                        Content = messageText,
                        Date = message.Date,
                        From = from,
                        RawMessage = JsonConvert.SerializeObject(message)
                    });
                }
            }
            catch (ExceededCurrentQuotaException)
            {
                Console.WriteLine("ExceededCurrentQuotaException ");
                await botClient.SendTextMessageAsync(
                           chatId: update.Message.Chat.Id,
                           text: "FOMODOG selhal, protože David Pomeranč nám dluží peníze za AI! Zatím, co čekáme na platbu, FOMODOG je ve stávce. Pošli pomeranče nebo peníze na záchranu FOMODOGu!",
                           cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await botClient.SendTextMessageAsync(
                           chatId: update.Message.Chat.Id,
                           text: "Ups, něco se pokazilo.",
                           cancellationToken: cancellationToken);
            }
        }

        string GetDateString(DateTime? date = null)
        {
            if (date == null)
                date = DateTime.Now;
            return date.Value.ToString("yyyy MMMM d hh:mm:ss");
        }

        string ReplaceVariables(string text)
        {
            return text
                .Replace("{DateTime.Now}", GetDateString());
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
