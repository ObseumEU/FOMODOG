using FomoDog.Context;
using FomoDog.Context.Models;
using FomoDog.GPT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using static FomoDog.GPT.ChatGPTClient;

namespace FomoDog
{
    public class DialogFlow : IDialogFlow
    {
        const string BOT_NAME = "FOMODOG";
        readonly IChatGPTClient _gpt;
        readonly IOptions<ChatbotOptions> _chatbotOptions;
        IChatRepository _chatRepository;
        ILogger<DialogFlow> _log;
        IMetadataDownloader _metadataDownloader;
        ITelegramBotClient _botClient;

        public DialogFlow(IOptions<ChatbotOptions> chatbotOptions, IChatGPTClient gpt, ILogger<DialogFlow> log, IChatRepository chatRepository, IMetadataDownloader metadataDownloader, ITelegramBotClient botClient)
        {
            _chatbotOptions = chatbotOptions;
            _gpt = gpt;
            _log = log;
            _chatRepository = chatRepository;
            _metadataDownloader = metadataDownloader;
            _botClient = botClient;
        }

        public async Task ReceiveMessage(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            try
            {
                var chatId = message.Chat.Id;
                var messageText = message.Text;

                using (var activity = OpenTelemetry.OpenTelemetry.Source.StartActivity("Receive message"))
                {
                    activity?.SetTag("chatId", chatId);
                    var from = $"{message?.From?.FirstName} {message?.From?.LastName}";

                    if (messageText.ToLower().Contains("mam fomo") || messageText == "42" || messageText.ToLower().Contains("mám fomo"))
                    {
                        await _botClient.SendTextMessageAsync(
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
                        if (messages == null)
                        {
                            messages = new List<ChatActivity>();
                        }

                        var activitiesTexts = messages.Select(m => m.ToString()).Select(ReplaceVariables).ToList();
                        var gptPrompt = _chatbotOptions.Value.ChatDetails.Replace("{DateTime.Now}", GetDateString()) + String.Join("\n", activitiesTexts);
                        string response = string.Empty;
                        using (OpenTelemetry.OpenTelemetry.Source.StartActivity("GPT get response"))
                        {
                            response = await _gpt.CallChatGpt(gptPrompt);
                        }
                        // Echo received message text

                        await _botClient.SendTextMessageAsync(
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
                    else
                    {
                        _log.LogInformation($"Received from telegfram '{JsonConvert.SerializeObject(message)}");
                        var links = ExtractHttpsLinks(messageText);
                        if (links?.Count() > 0)
                        {
                            foreach (var link in links)
                            {
                                var metadata = await _metadataDownloader.DownloadMetadata(link);
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
            }
            catch (ExceededCurrentQuotaException)
            {
                _log.LogError("ExceededCurrentQuotaException ");
                await _botClient.SendTextMessageAsync(
                           chatId: message.Chat.Id,
                           text: _chatbotOptions.Value.ExceededCurrentQuotaException,
                           cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
                await _botClient.SendTextMessageAsync(
                           chatId: message.Chat.Id,
                           text: _chatbotOptions.Value.FatalError,
                           cancellationToken: cancellationToken);
            }
        }

        private string[] ExtractHttpsLinks(string inputText)
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
    }
}
