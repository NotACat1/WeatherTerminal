using System.Text.Json;
using WeatherTerminal.Models;

namespace WeatherTerminal.Services;

/// <summary>
/// Клиент для работы с API OpenWeatherMap
/// Реализует IDisposable для корректного освобождения ресурсов HttpClient
/// </summary>
public sealed class WeatherApiClient : IDisposable
{    
    // Клиент HTTP для запросов к API
    private readonly HttpClient _client = new();
    
    // Логгер (используется Singleton через Instance)
    private readonly WeatherLogger _logger = WeatherLogger.Instance;
    
    // Настройки для десериализации JSON
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true // Разрешение нечувствительности к регистру
    };
    
    private readonly ConfigurationService _config = new();

    /// <summary>
    /// Получает текущие погодные данные для указанного города
    /// </summary>
    /// <param name="city">Название города</param>
    /// <returns>JSON-строка с данными или null при ошибке</returns>
    public async Task<string?> GetCurrentWeather(string city)
    {
        _logger.LogApiRequest(city); // Логирование запроса
        
        try
        {
            // Проверка кэша перед запросом к API
            var cachedData = _logger.GetCachedWeather(city);
            if (cachedData != null)
                return cachedData;

            var apiKey = _config.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API ключ не настроен");

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";
            
            _logger.Log($"Запрос погоды для {city}");
            using var response = await _client.GetAsync(url);
            
            // Обработка неуспешных статусных кодов
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.Log($"API Error: {errorContent}", LogLevel.ERROR);
                return null;
            }
            
            // Чтение и кэширование успешного ответа
            string json = await response.Content.ReadAsStringAsync();
            _logger.CacheWeatherData(city, json);
            
            return json;
        }
        catch (HttpRequestException ex)
        {
            // Специфичная обработка ошибок сети
            _logger.Log($"Ошибка сети: {ex.Message}", LogLevel.ERROR);
            return null;
        }
        catch (Exception ex)
        {
            // Общая обработка всех остальных ошибок
            _logger.Log($"Неожиданная ошибка: {ex.Message}", LogLevel.ERROR);
            return null;
        }
    }

    /// <summary>
    /// Получает прогноз погоды на 5 дней с интервалом 3 часа
    /// </summary>
    /// <param name="city">Название города</param>
    /// <returns>JSON-строка с прогнозом или null при ошибке</returns>
    public async Task<string?> GetWeatherForecast(string city)
    {
        try
        {
            var apiKey = _config.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API ключ не настроен");

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru&cnt=5";
            
            _logger.Log($"Запрос прогноза для {city}");
            using var response = await _client.GetAsync(url);
            
            // Гарантирует исключение при неуспешном статусе
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.Log($"Ошибка получения прогноза: {ex.Message}", LogLevel.ERROR);
            return null;
        }
    }

    /// <summary>
    /// Освобождает ресурсы HttpClient
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this); // Отменяет финализацию
    }
}

