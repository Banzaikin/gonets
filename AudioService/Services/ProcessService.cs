using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

public class ProcessService
{
    private readonly string _lpcnetPath;
    private readonly ILogger<ProcessService> _logger;
    private readonly WavConverter _wavConverter;

    public ProcessService(ILogger<ProcessService> logger, WavConverter wavConverter)
    {
        _logger = logger;
        _wavConverter = wavConverter;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _lpcnetPath = Path.Combine("libs", "win", "lpcnet_demo.exe");
            _logger.LogInformation("Используется исполняемый файл LPCNet для Windows");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Проверяем архитектуру процессора
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 || 
                RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                _lpcnetPath = Path.Combine("libs", "arm", "lpcnet_demo");
                _logger.LogInformation("Используется исполняемый файл LPCNet для Linux ARM");
            }
            else
            {
                _lpcnetPath = Path.Combine("libs", "linux", "lpcnet_demo");
                _logger.LogInformation("Используется исполняемый файл LPCNet для Linux x64");
            }
        }

        _logger.LogInformation($"Путь к LPCNet: {_lpcnetPath}");
    }

    public void Initialize()
	{
	    _logger.LogInformation("Инициализация LPCNet");

	    if (!File.Exists(_lpcnetPath))
	    {
		_logger.LogError("Файл LPCNet не найден: {Path}", _lpcnetPath);
		throw new FileNotFoundException("Исполняемый файл LPCNet не найден", _lpcnetPath);
	    }

	    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	    {
		try
		{
		    _logger.LogInformation("Попытка установить права на выполнение для LPCNet и вложенного бинарника");

		    var scriptsToChmod = new[]
		    {
		        _lpcnetPath,
		        Path.Combine(Path.GetDirectoryName(_lpcnetPath)!, ".libs", "lpcnet_demo")
		    };

		    foreach (var script in scriptsToChmod)
		    {
		        var chmod = new Process
		        {
		            StartInfo = new ProcessStartInfo
		            {
		                FileName = "/bin/chmod",
		                Arguments = $"+x \"{script}\"",
		                RedirectStandardOutput = true,
		                RedirectStandardError = true,
		                UseShellExecute = false,
		                CreateNoWindow = true
		            }
		        };
		        chmod.Start();
		        chmod.WaitForExit();
		        _logger.LogInformation($"Права на выполнение установлены: {script}");
		    }
		}
		catch (Exception ex)
		{
		    _logger.LogWarning(ex, "Не удалось установить права на выполнение LPCNet");
		}
	    }

	    // Тестовый запуск
	    try
	    {
		_logger.LogInformation("Пробный запуск LPCNet");
		RunProcess("--help");
		_logger.LogInformation("Пробный запуск LPCNet прошёл успешно");
	    }
	    catch (Exception ex)
	    {
		_logger.LogError(ex, "Пробный запуск LPCNet завершился с ошибкой");
	    }
	}


    public string EncodeToBase64(byte[] wavData)
    {
        _logger.LogInformation("Начат процесс кодирования");
        try
        {
            // Извлекаем PCM данные из WAV
            byte[] pcmData = _wavConverter.ExtractPcmFromWav(wavData);
            
            string inputPath = Path.GetTempFileName();
            File.WriteAllBytes(inputPath, pcmData);
            _logger.LogInformation($"Создан временный PCM-файл: {inputPath}");

            string outputPath = Path.GetTempFileName();
            RunProcess($"-encode {inputPath} {outputPath}");

            byte[] encoded = File.ReadAllBytes(outputPath);
            File.Delete(inputPath);
            File.Delete(outputPath);

            return Convert.ToBase64String(encoded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при кодировании");
            throw;
        }
    }

    public byte[] DecodeFromBase64(string base64)
    {
        _logger.LogInformation("Начат процесс декодирования");
        try
        {
            string inputPath = Path.GetTempFileName();
            string outputPath = Path.GetTempFileName() + ".wav";

            byte[] encodedData = Convert.FromBase64String(base64);
            File.WriteAllBytes(inputPath, encodedData);
            _logger.LogInformation($"Создан временный входной файл: {inputPath}");

            RunProcess($"-decode {inputPath} {outputPath}");
            _logger.LogInformation($"Декодирование LPCNet завершено");

            byte[] pcm = File.ReadAllBytes(outputPath);
            byte[] wav = _wavConverter.CreateWav(pcm);
            File.Delete(inputPath);
            File.Delete(outputPath);

            _logger.LogInformation($"Размер декодированного WAV: {wav.Length} байт");
            return wav;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при декодировании");
            throw;
        }
    }

    private void RunProcess(string arguments)
    {
        _logger.LogInformation($"Запуск процесса: {_lpcnetPath} {arguments}");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _lpcnetPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        _logger.LogInformation($"Вывод процесса: {output}");
        
        if (process.ExitCode != 0)
        {
            _logger.LogError($"Ошибка процесса: {error}");
            throw new Exception($"Ошибка процесса LPCNet: {error}");
        }
        else
        {
            _logger.LogInformation("Процесс завершился успешно");
        }
    }


}