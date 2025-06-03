using System.Text;
using System.Text.Json;
using Interfaces;
using Microsoft.Extensions.Configuration; // Adicione este using

namespace Services;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly IConfiguration _configuration; // Declaração do campo

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration; // Armazena a instância
        _apiKey = configuration["OpenAISettings:ApiKey"];
        _endpoint = configuration["OpenAISettings:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetChatResponseAsync(string prompt, string context = null)
    {
        var messages = new List<object>();
        if (!string.IsNullOrEmpty(context))
        {
            messages.Add(new { role = "system", content = context });
        }
        messages.Add(new { role = "user", content = prompt });

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = messages,
            temperature = 0.7
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseString);
        var choice = jsonResponse.RootElement.GetProperty("choices").EnumerateArray().FirstOrDefault();
        var message = choice.GetProperty("message").GetProperty("content").GetString();

        return message;
    }

    public async Task<byte[]> GetSpeechResponseAsync(string text)
    {
        // Implementação para ElevenLabs ou Azure AI Speech Text-to-Speech
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.elevenlabs.io/v1/text-to-speech/{_configuration["ElevenLabsSettings:VoiceId"]}");
        request.Headers.Add("xi-api-key", _configuration["ElevenLabsSettings:ApiKey"]);
        request.Content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<string> GetTextFromSpeechAsync(byte[] audioData)
    {
        // Implementação para ElevenLabs Speech-to-Text
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.elevenlabs.io/v1/speech-to-text");
        request.Headers.Add("xi-api-key", _configuration["ElevenLabsSettings:ApiKey"]);
        request.Content = new ByteArrayContent(audioData);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(result).RootElement.GetProperty("text").GetString();
    }

    public async Task<string> GetContextualResponseAboutProjectAsync(string prompt, string projectName)
    {
        var projectDetails = "Detalhes sobre o projeto 'Escola Conectada': Iniciativa para levar internet de alta velocidade e equipamentos digitais a todas as escolas públicas municipais. Status atual: Aguardando dotação orçamentária.";
        var fullContext = $"O projeto em questão é '{projectName}'. {projectDetails}. Responda à pergunta do usuário considerando o contexto do projeto: {prompt}";
        return await GetChatResponseAsync(prompt, fullContext);
    }
}