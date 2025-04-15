using WeatherTerminal.Models;
using WeatherTerminal.Services;

// Настройка консоли перед запуском приложения
Console.OutputEncoding = System.Text.Encoding.UTF8; // Поддержка Unicode символов
Console.Title = "Погодный терминал"; // Установка заголовка окна консоли

// Инициализация основных компонентов системы
var weatherTerminal = new WeatherFacade(); // Фасад для работы с погодой
var logger = WeatherLogger.Instance; // Логгер системы

try
{
    // Запуск основного цикла приложения
    await RunMainLoop(weatherTerminal);
}
catch (Exception ex)
{
    // Обработка непредвиденных исключений верхнего уровня
    logger.Log($"Критическая ошибка: {ex}", LogLevel.ERROR);
    Console.WriteLine("Произошла критическая ошибка. Подробности в логе.");
    Console.ReadKey();
}
finally
{
    // Гарантированное освобождение ресурсов
    weatherTerminal.Dispose();
    logger.Dispose();
}

/// <summary>
/// Основной цикл работы приложения
/// </summary>
/// <param name="terminal">Фасад для работы с погодными данными</param>
static async Task RunMainLoop(WeatherFacade terminal)
{
    while (true) // Бесконечный цикл меню
    {
        Console.Clear();
        DisplayMainMenu(); // Отображение главного меню

        string? input = Console.ReadLine()?.Trim(); // Чтение пользовательского ввода

        // Обработка выбора пользователя
        switch (input)
        {
            case "1": // Запрос погоды
                await HandleWeatherRequest(terminal);
                break;
            case "2": // Показ справки
                DisplayHelp();
                break;
            case "3": // Показ статистики
                DisplayLogStats();
                break;
            case "0": // Выход из приложения
                WeatherLogger.Instance.Log("=== Приложение закрыто ===");
                return;
            default: // Некорректный ввод
                Console.WriteLine("Неверный ввод! Пожалуйста, выберите 1-3 или 0 для выхода.");
                WeatherLogger.Instance.Log($"Некорректный ввод: {input}", LogLevel.WARNING);
                Console.ReadKey();
                break;
        }
    }
}

/// <summary>
/// Обрабатывает запрос погоды для указанного города
/// </summary>
/// <param name="terminal">Фасад для работы с погодными данными</param>
static async Task HandleWeatherRequest(WeatherFacade terminal)
{
    Console.Write("\nВведите город: ");
    string? city = Console.ReadLine()?.Trim();

    // Валидация ввода пользователя
    if (string.IsNullOrWhiteSpace(city))
    {
        Console.WriteLine("Название города не может быть пустым!");
        Console.ReadKey();
        return;
    }

    // Запрос и отображение погодных данных
    await terminal.DisplayWeatherInfo(city);
}

/// <summary>
/// Отображает главное меню приложения
/// </summary>
static void DisplayMainMenu()
{
    Console.WriteLine("╔════════════════ Погодный терминал Pro ═══════════════╗");
    Console.WriteLine("║                                                      ║");
    Console.WriteLine("║ 1. Узнать погоду                                     ║");
    Console.WriteLine("║ 2. Помощь                                            ║");
    Console.WriteLine("║ 3. Статистика запросов                               ║");
    Console.WriteLine("║                                                      ║");
    Console.WriteLine("║ 0. Выход                                             ║");
    Console.WriteLine("║                                                      ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝");
    Console.Write("\nВыберите действие: ");
}

/// <summary>
/// Отображает справочную информацию по работе с приложением
/// </summary>
static void DisplayHelp()
{
    Console.Clear();
    Console.WriteLine("╔════════════════════ Помощь ═════════════════════╗");
    Console.WriteLine("║                                                 ║");
    Console.WriteLine("║ 1. Введите название города на русском или       ║");
    Console.WriteLine("║    английском языке (например: Москва или       ║");
    Console.WriteLine("║    London)                                      ║");
    Console.WriteLine("║                                                 ║");
    Console.WriteLine("║ 2. Для городов с одинаковыми названиями         ║");
    Console.WriteLine("║    укажите страну через запятую (Кемерово,RU)   ║");
    Console.WriteLine("║                                                 ║");
    Console.WriteLine("║ 3. Приложение кеширует запросы на 30 минут      ║");
    Console.WriteLine("║                                                 ║");
    Console.WriteLine("╚═════════════════════════════════════════════════╝");
    Console.WriteLine("\nНажмите любую клавишу для возврата...");
    Console.ReadKey();
}

/// <summary>
/// Отображает статистику запросов к API
/// </summary>
static void DisplayLogStats()
{
    Console.Clear();
    string statsFile = "api_key_log.txt"; // Файл хранения статистики
    
    // Проверка существования файла статистики
    if (!File.Exists(statsFile))
    {
        Console.WriteLine("Статистика недоступна");
        Console.ReadKey();
        return;
    }

    try
    {
        // Чтение и анализ данных статистики
        var requests = File.ReadAllLines(statsFile);
        int totalRequests = requests.Length - 1; // Первая строка - заголовок
        
        // Форматированный вывод статистики
        Console.WriteLine("╔════════════════ Статистика запросов ═══════════════╗");
        Console.WriteLine($"║ Всего запросов: {totalRequests,34} ║");
        
        // Вывод информации о последнем запросе
        if (totalRequests > 0)
        {
            var lastRequest = requests[^1].Split('-')[1].Trim();
            Console.WriteLine($"║ Последний запрос: {lastRequest,-32} ║");
        }
        
        Console.WriteLine("╚════════════════════════════════════════════════════╝");
    }
    catch (Exception ex)
    {
        // Обработка ошибок при работе с файлом статистики
        WeatherLogger.Instance.Log($"Ошибка чтения статистики: {ex.Message}", LogLevel.ERROR);
        Console.WriteLine("Ошибка при получении статистики");
    }

    Console.WriteLine("\nНажмите любую клавишу для возврата...");
    Console.ReadKey();
}
