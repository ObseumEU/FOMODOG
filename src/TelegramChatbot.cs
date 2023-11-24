/*
Hello dear code explorer,
Every line here is unique, special, and dreadfully inefficient. They say "Good things take time", so if time inversely correlates 
with quality... Well, you can draw your own conclusions.  
So, in conclusion, let me apologize:
*/

using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace FomoDog
{
    public class TelegramChatbot
    {
        ITelegramBotClient _botClient;
        IServiceProvider _provider;

        public TelegramChatbot(ITelegramBotClient botClient, IServiceProvider provider)
        {
            _botClient = botClient;
            _provider = provider;
        }

        public async Task Run()
        {

            using CancellationTokenSource cts = new(); // So much for running forever.

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>() // receive all update types
            };
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
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


        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message) // Our bot is very picky about the types of messages it accepts.
                return;
            // Only process text messages
            if (message.Text is not { })
                return;

            using (var scope = _provider.CreateScope())
            {
                var flow = scope.ServiceProvider.GetRequiredService<DialogFlow>();
                await flow.ReceiveMessage(message, cancellationToken);
            }
        }
    }
}
