using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using System;


[ApiController]
[Route("[controller]")]
public class EncodeController : ControllerBase
{
    private readonly ProcessService _audio;
    private readonly ILogger<EncodeController> _logger;

    public EncodeController(ProcessService audio, ILogger<EncodeController> logger)
    {
        _audio = audio;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Encode(IFormFile file)
    {
        _logger.LogInformation($"Получен запрос на кодирование. Файл: {file?.FileName}, Размер: {file?.Length} байт");
        
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Загружен недопустимый файл");
            return BadRequest("Недопустимый файл");
        }

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            string result = _audio.EncodeToBase64(ms.ToArray());
            _logger.LogInformation($"Кодирование успешно. Размер результата: {result.Length} символов");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при кодировании");
            return StatusCode(500, "Ошибка при кодировании");
        }
    }
}

