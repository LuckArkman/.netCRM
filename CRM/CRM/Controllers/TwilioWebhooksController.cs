using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;
// Necessário para ClaimTypes

namespace Controllers
{
    // Não precisa de Authorize, Twilio usa sua própria autenticação/assinatura de requisição
    [ApiController]
    [Route("api/webhooks/twilio")]
    public class TwilioWebhooksController : TwilioController
    {
        private readonly IAIService _aiService;
        private readonly ISolicitacaoService _solicitacaoService; // Para buscar status de protocolo, por exemplo
        // private readonly IMultichannelService _multichannelService; // Se precisar enviar mensagens de volta para Twilio

        public TwilioWebhooksController(IAIService aiService, ISolicitacaoService solicitacaoService)
        {
            _aiService = aiService;
            _solicitacaoService = solicitacaoService;
            // _multichannelService = multichannelService;
        }

        // Webhook para mensagens de WhatsApp
        [HttpPost("whatsapp")]
        public async Task<TwiMLResult> WhatsAppMessage([FromForm] string Body, [FromForm] string From)
        {
            var messagingResponse = new MessagingResponse();
            var userMessage = Body.Trim();
            var fromNumber = From; // Ex: whatsapp:+5511999999999

            string aiResponseText = "Desculpe, não entendi. Poderia reformular?";

            // Lógica para processar a mensagem do usuário com IA
            if (userMessage.ToLower().StartsWith("protocolo "))
            {
                var protocolo = userMessage.Substring("protocolo ".Length).Trim();
                var solicitacao = await _solicitacaoService.GetSolicitacaoByProtocoloAsync(protocolo);
                if (solicitacao != null)
                {
                    aiResponseText = $"O status da solicitação {solicitacao.Protocolo} ({solicitacao.Titulo}) é: {solicitacao.Status}. Última atualização em {solicitacao.DataUltimaAtualizacao:dd/MM/yyyy}.";
                }
                else
                {
                    aiResponseText = $"Protocolo '{protocolo}' não encontrado.";
                }
            }
            else
            {
                // Envia para o serviço de IA geral
                aiResponseText = await _aiService.GetChatResponseAsync(userMessage, "Você é um assistente do Gabinete Digital de um vereador. Forneça informações sobre serviços públicos e projetos do vereador.");
            }

            messagingResponse.Message(aiResponseText);
            return TwiML(messagingResponse);
        }

        // Webhook para chamadas de voz de entrada
        [HttpPost("voice")]
        public async Task<TwiMLResult> VoiceCall([FromForm] string CallSid, [FromForm] string From, [FromForm] string Digits = null)
        {
            var response = new VoiceResponse();

            if (!string.IsNullOrEmpty(Digits))
            {
                // Lógica para DTMF (toques no teclado)
                response.Say("Você digitou " + Digits, language: "pt-BR");
            }
            else
            {
                // Responde com mensagem de boas-vindas e coleta fala do usuário
                response.Say("Bem-vindo ao Gabinete Digital. Por favor, diga em poucas palavras o motivo do seu contato.", language: "pt-BR");
                response.Record(action: new Uri(Url.Action("VoiceRecording", "TwilioWebhooks")), maxLength: 10, finishOnKey: "#"); // Grava até 10 segundos ou até #
            }

            return TwiML(response);
        }

        [HttpPost("voice-recording")]
        public async Task<TwiMLResult> VoiceRecording([FromForm] string RecordingUrl)
        {
            var response = new VoiceResponse();

            if (!string.IsNullOrEmpty(RecordingUrl))
            {
                // Baixar o áudio da URL da gravação
                using var httpClient = new HttpClient();
                var audioBytes = await httpClient.GetByteArrayAsync(RecordingUrl);

                // Converter áudio para texto usando IA
                var textFromSpeech = await _aiService.GetTextFromSpeechAsync(audioBytes);

                string aiResponseText;
                if (!string.IsNullOrWhiteSpace(textFromSpeech))
                {
                    // Processar a intenção do texto com IA
                    aiResponseText = await _aiService.GetChatResponseAsync(textFromSpeech, "Você é um assistente de voz do Gabinete Digital. Responda de forma concisa e útil.");
                }
                else
                {
                    aiResponseText = "Não consegui entender sua fala. Por favor, tente novamente.";
                }

                // Converter resposta da IA para voz
                var audioResponseBytes = await _aiService.GetSpeechResponseAsync(aiResponseText);
                // A Twilio TwiML suporta <Say> para texto simples, para áudio pré-gerado, você precisaria de um endpoint
                // que sirva o áudio e usar <Play url="[URL_DO_AUDIO_GERADO_PELA_IA]"/>
                // Para simplificar, usamos <Say> aqui com ElevenLabs no serviço.
                response.Say(aiResponseText, language: "pt-BR");
            }
            else
            {
                response.Say("Não recebi uma gravação. Conectando você a um atendente.", language: "pt-BR");
                // Aqui você pode adicionar lógica para transferir a chamada para um atendente humano
            }

            return TwiML(response);
        }
    }
}