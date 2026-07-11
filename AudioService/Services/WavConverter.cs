using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

public class WavConverter
{
    private readonly ILogger<WavConverter> _logger;

    public WavConverter(ILogger<WavConverter> logger)
    {
        _logger = logger;
    }

    public byte[] ExtractPcmFromWav(byte[] wavData)
    {
        _logger.LogInformation("Извлечение PCM из WAV-данных");
        try
        {
            // Проверяем сигнатуру WAV файла
            if (wavData.Length < 12 || 
                Encoding.ASCII.GetString(wavData, 0, 4) != "RIFF" ||
                Encoding.ASCII.GetString(wavData, 8, 4) != "WAVE")
            {
                throw new ArgumentException("Неверный формат WAV-файла");
            }

            // Извлекаем размер данных из заголовка
            int dataSize = BitConverter.ToInt32(wavData, 40);
            
            // Проверяем, что данных достаточно
            if (wavData.Length < 44 + dataSize)
            {
                throw new ArgumentException("WAV-файл повреждён или неполный");
            }

            // Копируем PCM данные
            byte[] pcm = new byte[dataSize];
            Buffer.BlockCopy(wavData, 44, pcm, 0, dataSize);
            
            _logger.LogInformation($"Извлечено PCM-данных: {pcm.Length} байт");
            return pcm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при извлечении PCM-данных");
            throw;
        }
    }

    public byte[] CreateWav(byte[] pcmData, int sampleRate = 16000, short channels = 1, short bitsPerSample = 16)
    {
        _logger.LogInformation("Создание WAV из PCM-данных");
        try
        {
            int bytesPerSample = bitsPerSample / 8;
            int byteRate = sampleRate * channels * bytesPerSample;
            int blockAlign = channels * bytesPerSample;
            int dataSize = pcmData.Length;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // RIFF-заголовок
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize); // Размер чанка
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // Подчанк fmt
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Размер подчанка
            writer.Write((short)1); // Формат аудио (PCM)
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write(bitsPerSample);

            // Подчанк data
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            // PCM-данные
            writer.Write(pcmData);

            var wav = ms.ToArray();
            _logger.LogInformation($"Создан WAV-файл: {wav.Length} байт");
            return wav;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании WAV-файла");
            throw;
        }
    }
}
