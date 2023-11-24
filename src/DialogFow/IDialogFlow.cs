namespace FomoDog
{
    public interface IDialogFlow
    {
        Task ReceiveMessage(Telegram.Bot.Types.Message message, CancellationToken cancellationToken);
    }
}