using System.Text;
using System.Text.Json;
using Interfaces;

namespace Services;

public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
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
                model = "gpt-3.5-turbo", // Ou "gpt-4"
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
            // Requer pacotes e configurações adicionais para ElevenLabs ou Azure
            // Exemplo MOCK:
            return Encoding.UTF8.GetBytes($"Mock audio for: {text}");
        }

        public async Task<string> GetTextFromSpeechAsync(byte[] audioData)
        {
            // Implementação para Azure AI Speech Speech-to-Text ou similar
            // Requer pacotes e configurações adicionais
            // Exemplo MOCK:
            return "Mock text from speech: Olá, como posso ajudar?";
        }

        public async Task<string> GetContextualResponseAboutProjectAsync(string prompt, string projectName)
        {
            // Aqui você buscaria detalhes do projeto no seu DB
            // e construiria um prompt mais elaborado para a IA
            var projectDetails = "Detalhes sobre o projeto 'Escola Conectada': Iniciativa para levar internet de alta velocidade e equipamentos digitais a todas as escolas públicas municipais. Status atual: Aguardando dotação orçamentária.";
            var fullContext = $"O projeto em questão é '{projectName}'. {projectDetails}. Responda à pergunta do usuário considerando o contexto do projeto: {prompt}";
            return await GetChatResponseAsync(prompt, fullContext);
        }
    }