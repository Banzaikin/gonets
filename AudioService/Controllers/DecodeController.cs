using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Net.Http.Headers; 

[ApiController]
[Route("[controller]")]
public class DecodeController : ControllerBase
{
    private readonly ProcessService _audio;
    private readonly ILogger<DecodeController> _logger;

    public DecodeController(ProcessService audio, ILogger<DecodeController> logger)
    {
        _audio = audio;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Decode([FromBody] string base64)
    {
        _logger.LogInformation("Получен запрос на декодирование");
        try
        {
            byte[] wav = _audio.DecodeFromBase64(base64);
            _logger.LogInformation($"Размер декодированного WAV: {wav.Length} байт");
            // Устанавливаем Content-Disposition для принудительного скачивания
            var contentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "decoded.wav"
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            
            return File(wav, "audio/wav");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при декодировании");
            return StatusCode(500, "Ошибка при декодировании");
        }
    }
}
