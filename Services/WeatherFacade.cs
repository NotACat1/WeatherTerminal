using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherTerminal.Models;

namespace WeatherTerminal.Services;

/// <summary>
/// Фасадный класс для работы с погодными данными.
/// Инкапсулирует логику получения, обработки и отображения погодной информации.
/// </summary>
public sealed class WeatherFacade : IDisposable
{
    // Клиент для работы с API погоды
    private readonly WeatherApiClient _apiClient = new();
    
    // Логгер для записи событий и ошибок
    private readonly WeatherLogger _logger = WeatherLogger.Instance;
    
    // Настройки для десериализации JSON
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true, // Игнорировать регистр свойств
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Имена свойств в camelCase
        WriteIndented = true, // Форматированный вывод
        Converters = { new JsonStringEnumConverter() } // Конвертер для перечислений
    };

    /// <summary>
    /// Основной метод отображения погодной информации.
    /// Управляет всем процессом: получение данных, отображение, рекомендации.
    /// </summary>
    /// <param name="city">Название города для запроса погоды</param>
    public async Task DisplayWeatherInfo(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine("Название города не может быть пустым!");
            return;
        }

        try
        {
            Console.Clear();
            DisplayHeader($"Погода в {city.ToUpper()}");

            var currentWeather = await GetAndDisplayCurrentWeather(city);
            if (currentWeather is null) return;

            DisplayRecommendations(currentWeather);

            if (currentWeather.Main.Temperature > 25 || currentWeather.Description.Contains("дождь"))
            {
                await DisplayExtendedForecast(city);
            }

            DisplayFooter();
        }
        catch (Exception ex)
        {
            _logger.Log($"Ошибка отображения погоды: {ex.Message}", LogLevel.ERROR);
            Console.WriteLine("Произошла ошибка при получении данных о погоде.");
        }
    }

    /// <summary>
    /// Получает и отображает текущую погоду для указанного города.
    /// </summary>
    /// <param name="city">Название города</param>
    /// <returns>Объект WeatherData или null при ошибке</returns>
    private async Task<WeatherData?> GetAndDisplayCurrentWeather(string city)
    {
        var json = await _apiClient.GetCurrentWeather(city);
        if (json is null)
        {
            Console.WriteLine("Не удалось получить данные о погоде");
            return null;
        }

        try
        {
            var weatherData = JsonSerializer.Deserialize<WeatherData>(json, _jsonOptions);
            if (weatherData is null)
            {
                _logger.Log("Не удалось десериализовать данные о погоде", LogLevel.ERROR);
                return null;
            }

            DisplayWeatherCard(weatherData);
            return weatherData;
        }
        catch (JsonException ex)
        {
            _logger.Log($"Ошибка парсинга JSON: {ex.Message}", LogLevel.ERROR);
            Console.WriteLine("Ошибка обработки данных о погоде");
            return null;
        }
    }

    /// <summary>
    /// Форматирует и выводит в консоль карточку с текущей погодой.
    /// </summary>
    /// <param name="data">Данные о погоде</param>
    private static void DisplayWeatherCard(WeatherData data)
    {
        string cityLine =     $"│ {data.City,-39} {GetWeatherIcon(data.Icon),4}  │";
        string tempLine =     $"│ Температура: {data.Main.Temperature,5:0.#}°C (ощущается как {data.Main.FeelsLike,5:0.#}°C) │";
        string humidityLine = $"│ Влажность:    {data.Main.Humidity,29}% │";
        string windLine =     $"│ Ветер:        {data.Wind.Speed,26:0.#} м/с │";
        string descLine =     $"│ Описание:     {data.Description,-30} │";

        Console.WriteLine("\n┌──────────────────────────────────────────────┐");
        Console.WriteLine(cityLine);
        Console.WriteLine("├──────────────────────────────────────────────┤");
        Console.WriteLine(tempLine);
        Console.WriteLine(humidityLine);
        Console.WriteLine(windLine);
        Console.WriteLine(descLine);
        Console.WriteLine("└──────────────────────────────────────────────┘");
    }

    /// <summary>
    /// Получает и отображает расширенный прогноз погоды.
    /// Вызывается только при определенных условиях (жарко/дождь/сильный ветер).
    /// </summary>
    /// <param name="city">Название города</param>
    private async Task DisplayExtendedForecast(string city)
    {
        Console.WriteLine("\nПолучаем расширенный прогноз...");
        
        var forecastJson = await _apiClient.GetWeatherForecast(city);
        if (forecastJson is null) return;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(forecastJson);
            var forecasts = doc.RootElement.GetProperty("list");

            Console.WriteLine("\n┌───────────────────── Прогноз на 12 часов ─────────────────────┐");
            Console.WriteLine("│ Дата/Время    Температура  Погода                     Ветер   │");
            Console.WriteLine("├───────────────────────────────────────────────────────────────┤");

            foreach (var forecast in forecasts.EnumerateArray())
            {
                var dt = DateTime.Parse(forecast.GetProperty("dt_txt").GetString()!);
                var temp = forecast.GetProperty("main").GetProperty("temp").GetDouble();
                var desc = forecast.GetProperty("weather")[0].GetProperty("description").GetString()!;
                var wind = forecast.GetProperty("wind").GetProperty("speed").GetDouble();

                Console.WriteLine($"│ {dt:dd.MM HH:mm}  {temp,10:0.#}°C  {desc,-25} {wind,4:0.#} м/с │");
            }

            Console.WriteLine("└───────────────────────────────────────────────────────────────┘");
        }
        catch (JsonException ex)
        {
            _logger.Log($"Ошибка парсинга прогноза: {ex.Message}", LogLevel.ERROR);
            Console.WriteLine("Ошибка обработки прогноза погоды");
        }
    }

    /// <summary>
    /// Отображает блок с рекомендациями на основе текущей погоды.
    /// </summary>
    /// <param name="data">Данные о погоде</param>
    private static void DisplayRecommendations(WeatherData data)
    {
        string clothesRec = GetClothesRecommendation(data.Main.Temperature);
        string umbrellaRec = GetUmbrellaRecommendation(data.Description);
        string sunRec = GetSunProtectionRecommendation(data.Main.Temperature);

        Console.WriteLine("\n╔══════════════════════════════════════════════╗");
        Console.WriteLine($"║ {clothesRec,-45} ║");
        Console.WriteLine($"║ {umbrellaRec,-44} ║");
        Console.WriteLine($"║ {sunRec,-44} ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
    }

    /// <summary>
    /// Генерирует рекомендацию по одежде на основе температуры.
    /// </summary>
    /// <param name="temp">Температура в °C</param>
    /// <returns>Текст рекомендации</returns>
    private static string GetClothesRecommendation(double temp) => temp switch
    {
        < -10 => "❄️ Одевайтесь очень тепло: пуховик, шапка, шарф",
        < 0 => "⛄ Теплая зимняя одежда обязательна",
        < 10 => "🍂 Куртка или пальто, головной убор",
        < 18 => "🌧️ Легкая куртка или кофта",
        < 25 => "🌤️ Футболка + кофта на вечер",
        _ => "☀️ Легкая одежда, головной убор от солнца"
    };

    /// <summary>
    /// Определяет нужен ли зонт на основе описания погоды.
    /// </summary>
    /// <param name="description">Описание погоды</param>
    /// <returns>Рекомендация по зонту</returns>
    private static string GetUmbrellaRecommendation(string description) => 
        description.Contains("дождь") ? "☔ Возьмите зонт!" : "🌂 Зонт не нужен";

    /// <summary>
    /// Определяет нужна ли защита от солнца.
    /// </summary>
    /// <param name="temp">Температура в °C</param>
    /// <returns>Рекомендация по защите от солнца</returns>
    private static string GetSunProtectionRecommendation(double temp) => 
        temp > 25 ? "🧴 Используйте солнцезащитный крем" : "⛅ Защита от солнца не требуется";

    /// <summary>
    /// Возвращает Unicode-символ иконки погоды по коду от API.
    /// </summary>
    /// <param name="iconCode">Код иконки от OpenWeatherMap</param>
    /// <returns>Соответствующий символ</returns>
    private static string GetWeatherIcon(string iconCode) => iconCode switch
    {
        "01d" => "☀️",  // Ясно (день)
        "01n" => "🌙",  // Ясно (ночь)
        "02d" or "02n" => "⛅",  // Небольшая облачность
        "03d" or "03n" => "☁️",  // Облачно
        "04d" or "04n" => "🌫️",  // Пасмурно
        "09d" or "09n" => "🌧️",  // Дождь
        "10d" or "10n" => "🌦️",  // Ливень
        "11d" or "11n" => "⛈️",  // Гроза
        "13d" or "13n" => "❄️",  // Снег
        "50d" or "50n" => "🌫️",  // Туман
        _ => "🌈"               // Другое
    };

    /// <summary>
    /// Выводит заголовок приложения с названием города.
    /// </summary>
    /// <param name="title">Текст заголовка</param>
    private static void DisplayHeader(string title)
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine($"║ {title.PadRight(44).Substring(0, 44)} ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
    }

    /// <summary>
    /// Выводит подвал интерфейса с предложением продолжить.
    /// </summary>
    private static void DisplayFooter()
    {
        Console.WriteLine("\nДля продолжения нажмите любую клавишу...");
        Console.ReadKey();
    }

    /// <summary>
    /// Освобождает ресурсы HttpClient.
    /// </summary>
    public void Dispose()
    {
        _apiClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
