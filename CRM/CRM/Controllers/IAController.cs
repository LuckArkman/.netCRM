using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IAController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<IAController> _logger;

    public IAController(IAIService aiService, ILogger<IAController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("speech-to-text")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SpeechToText([FromForm] IFormFile audioFile)
    {
        _logger.LogInformation("Recebida requisição SpeechToText com arquivo: {FileName}", audioFile?.FileName);

        if (audioFile == null || audioFile.Length == 0)
        {
            _logger.LogWarning("Arquivo de áudio inválido recebido.");
            return BadRequest("Arquivo de áudio inválido.");
        }

        using var memoryStream = new MemoryStream();
        await audioFile.CopyToAsync(memoryStream);
        var audioData = memoryStream.ToArray();

        var text = await _aiService.GetTextFromSpeechAsync(audioData);
        _logger.LogInformation("Resultado de speech-to-text: {Text}", text);

        return Ok(new { Text = text });
    }
}