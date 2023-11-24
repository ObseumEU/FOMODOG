using FomoDog.Context;
using FomoDog.DialogFow;
using FomoDog.GPT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot;
using static FomoDog.GPT.ChatGPTClient;

namespace FomoDog
{
    public class DialogFlow
    {
        const string BOT_NAME = "FOMODOG";
        readonly ChatGPTClient _gpt;
        readonly IOptions<ChatbotOptions> _chatbotOptions;
        readonly IOptions<TelegramOptions> _telegramOptions;
        IChatRepository _chatRepository;
        ILogger<DialogFlow> _log;
        MetadataDownloader _metadataDownloader;
        ITelegramBotClient _botClient;

        public DialogFlow(IOptions<ChatbotOptions> chatbotOptions, ChatGPTClient gpt, IOptions<TelegramOptions> telegramOptions, ILogger<DialogFlow> log, IChatRepository chatRepository, MetadataDownloader metadataDownloader, ITelegramBotClient botClient)
        {
            _chatbotOptions = chatbotOptions;
            _telegramOptions = telegramOptions;
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
                    activity.SetTag("chatId", chatId);
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

                        try
                        {
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
                        catch (Exception ex)
                        {
                            //DRY? I dont care.
                            _log.LogError(ex.Message);
                            var response = await _gpt.CallChatGpt(_chatbotOptions.Value.ChatDetails.Replace("{DateTime.Now}", DateTime.Now.ToString()) + string.Join("\n", messages));
                            // Echo received message text
                            await _botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: response,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        _log.LogInformation($"Received from telegfram '{JsonConvert.SerializeObject(message)}");
                        var links = DialogFlowHelpers.ExtractHttpsLinks(messageText);
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
                _log.LogError(ex.Message);
                await _botClient.SendTextMessageAsync(
                           chatId: message.Chat.Id,
                           text: _chatbotOptions.Value.FatalError,
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
    }
}
