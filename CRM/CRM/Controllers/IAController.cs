using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize] // Pode ser mais permissivo para chatbot público
    [ApiController]
    [Route("api/[controller]")]
    public class IAController : ControllerBase
    {
        private readonly IAIService _aiService;

        public IAController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        [AllowAnonymous] // Chatbot pode ser público
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChatResponse([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("O prompt não pode ser vazio.");
            }
            var response = await _aiService.GetChatResponseAsync(request.Prompt, request.Context);
            return Ok(response);
        }

        [HttpPost("speech-to-text")]
        [Consumes("audio/wav", "audio/mpeg")] // Tipos de áudio aceitos
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SpeechToText([FromForm] IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("Nenhum arquivo de áudio enviado.");
            }

            using var ms = new MemoryStream();
            await audioFile.CopyToAsync(ms);
            var audioData = ms.ToArray();

            var text = await _aiService.GetTextFromSpeechAsync(audioData);
            return Ok(text);
        }

        [HttpGet("text-to-speech")]
        [Produces("audio/mpeg")] // Ou "audio/wav" dependendo do ElevenLabs/Azure output
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TextToSpeech([FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("O texto não pode ser vazio.");
            }
            var audioBytes = await _aiService.GetSpeechResponseAsync(text);
            return File(audioBytes, "audio/mpeg"); // Ajuste o tipo MIME conforme a IA
        }

        // DTO para requisição de chat
        public class ChatRequestDto
        {
            public string Prompt { get; set; }
            public string Context { get; set; }
        }
    }
}