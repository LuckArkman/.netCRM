namespace Interfaces
{
    public interface IAIService
    {
        Task<string> GetChatResponseAsync(string prompt, string context = null);
        Task<byte[]> GetSpeechResponseAsync(string text); // Text-to-Speech
        Task<string> GetTextFromSpeechAsync(byte[] audioData); // Speech-to-Text
        Task<string> GetContextualResponseAboutProjectAsync(string prompt, string projectName);
    }
}