using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherTerminal.Models;

namespace WeatherTerminal.Services;

/// <summary>
/// Потокобезопасный Singleton-класс для логгирования и кеширования погодных данных.
/// Реализует IDisposable для корректного управления ресурсами.
/// </summary>
public sealed class WeatherLogger : IDisposable
{
    private static WeatherLogger? _instance;
    private static readonly object _lock = new();
    private readonly string _logDirectory = "WeatherLogs";
    private readonly string _cacheFile = "weather_cache.json";
    private readonly string _apiKeyLog = "api_key_log.txt";
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(30);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Приватный конструктор для реализации паттерна Singleton
    /// </summary>
    private WeatherLogger()
    {
        InitializeLogger();
    }

    /// <summary>
    /// Единственный экземпляр логгера (реализация Singleton с double-check locking)
    /// </summary>
    public static WeatherLogger Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new WeatherLogger();
            }
        }
    }

    /// <summary>
    /// Инициализирует систему логгирования:
    /// - Создает директорию для логов
    /// - Создает файл лога за текущий день
    /// - Создает файл для статистики использования API ключа
    /// </summary>
    private void InitializeLogger()
    {
        try
        {
            Directory.CreateDirectory(_logDirectory);
            
            string todayLog = GetTodayLogFilePath();
            if (!File.Exists(todayLog))
            {
                File.WriteAllText(todayLog, $"=== Weather Terminal Log {DateTime.Now:yyyy-MM-dd} ===\n");
            }

            if (!File.Exists(_apiKeyLog))
            {
                File.WriteAllText(_apiKeyLog, "API Key Usage Statistics\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logger initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Генерирует путь к файлу лога за текущий день
    /// </summary>
    /// <returns>Полный путь к файлу лога</returns>
    private string GetTodayLogFilePath() => 
        Path.Combine(_logDirectory, $"weather_{DateTime.Now:yyyyMMdd}.log");

    /// <summary>
    /// Записывает сообщение в лог-файл и выводит в консоль (для WARNING и ERROR)
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="level">Уровень важности (по умолчанию INFO)</param>
    public void Log(string message, LogLevel level = LogLevel.INFO)
    {
        try
        {
            string todayLog = GetTodayLogFilePath();
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}\n";
            
            File.AppendAllText(todayLog, entry);
            
            // Вывод предупреждений и ошибок в консоль с цветовым выделением
            if (level >= LogLevel.WARNING)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = level == LogLevel.ERROR ? ConsoleColor.Red : ConsoleColor.Yellow;
                Console.WriteLine(entry.Trim());
                Console.ForegroundColor = originalColor;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log: {ex.Message}");
        }
    }

    /// <summary>
    /// Логирует запрос к API для статистики использования
    /// </summary>
    /// <param name="city">Название запрошенного города</param>
    public void LogApiRequest(string city)
    {
        try
        {
            string stats = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Request for: {city}\n";
            File.AppendAllText(_apiKeyLog, stats);
        }
        catch (Exception ex)
        {
            Log($"Failed to log API request: {ex.Message}", LogLevel.ERROR);
        }
    }

    /// <summary>
    /// Кеширует погодные данные для указанного города
    /// </summary>
    /// <param name="city">Название города</param>
    /// <param name="jsonData">Данные в JSON-формате</param>
    public void CacheWeatherData(string city, string jsonData)
    {
        try
        {
            Dictionary<string, CacheEntry> cache;
            
            if (File.Exists(_cacheFile))
            {
                var json = File.ReadAllText(_cacheFile);
                cache = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json) ?? new();
            }
            else
            {
                cache = new();
            }

            // Обновляем или добавляем запись
            cache[city.ToLowerInvariant()] = new CacheEntry
            {
                Data = jsonData,
                Timestamp = DateTime.Now
            };

            File.WriteAllText(_cacheFile, JsonSerializer.Serialize(cache, _jsonOptions));
            Log($"Данные для {city} сохранены в кеш", LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            Log($"Ошибка кеширования: {ex.Message}", LogLevel.ERROR);
        }
    }

    /// <summary>
    /// Получает кешированные данные для указанного города
    /// </summary>
    /// <param name="city">Название города</param>
    /// <returns>JSON-данные или null если кеш недействителен</returns>
    public string? GetCachedWeather(string city)
    {
        try
        {
            if (!File.Exists(_cacheFile))
                return null;

            var json = File.ReadAllText(_cacheFile);
            var cache = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json);
            
            if (cache == null || !cache.TryGetValue(city.ToLowerInvariant(), out var entry))
                return null;

            // Проверяем срок действия кеша
            if (DateTime.Now - entry.Timestamp > _cacheLifetime)
            {
                // Удаляем просроченную запись
                cache.Remove(city.ToLowerInvariant());
                File.WriteAllText(_cacheFile, JsonSerializer.Serialize(cache, _jsonOptions));
                return null;
            }

            Log($"Использованы кешированные данные для {city}", LogLevel.INFO);
            return entry.Data;
        }
        catch (Exception ex)
        {
            Log($"Ошибка чтения кеша: {ex.Message}", LogLevel.ERROR);
            return null;
        }
    }
    
    private class CacheEntry
    {
        public string Data { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Освобождает ресурсы логгера (сбрасывает Singleton-экземпляр)
    /// </summary>
    public void Dispose()
    {
        _instance = null;
    }
}
