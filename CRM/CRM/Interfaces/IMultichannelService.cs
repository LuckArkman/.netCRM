namespace Interfaces
{
    public interface IMultichannelService
    {
        Task SendWhatsAppMessageAsync(string to, string message);
        Task SendVoiceCallAsync(string to, string message);
    }
}