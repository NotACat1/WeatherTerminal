# 🌦️ Weather Terminal

**Консольное приложение для получения актуальной информации о погоде с интеллектуальными рекомендациями**

[![.NET Version](https://img.shields.io/badge/.NET-6.0-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📌 Возможности

✔️ Точные погодные данные в реальном времени  
✔️ Подробная информация: температура, влажность, ветер  
✔️ Умные рекомендации по одежде и аксессуарам  
✔️ Кеширование запросов (30 минут)  
✔️ Полноценная система логирования  
✔️ Красивый Unicode-интерфейс с иконками  
✔️ Статистика использования API  

## 🚀 Быстрый старт

### Требования
- [.NET 6.0](https://dotnet.microsoft.com/download)
- API ключ от [OpenWeatherMap](https://openweathermap.org/api)

### Установка
1. Клонируйте репозиторий:
```bash
git https://github.com/NotACat1/WeatherTerminal.git
cd WeatherTerminal
```

2. Настройте API ключ (выберите один способ):
```bash
# Способ 1: Через консоль
dotnet run -- --set-api-key YOUR_API_KEY

# Способ 2: Вручную в appsettings.json
echo '{"ApiKey":"YOUR_API_KEY"}' > WeatherTerminal/bin/Debug/net6.0/appsettings.json
```

3. Запустите приложение:
```bash
dotnet run
```

## 🖥️ Использование

Главное меню:
```
╔════════════════ Погодный терминал Pro ═══════════════╗
║                                                      ║
║ 1. Узнать погоду                                     ║
║ 2. Помощь                                            ║
║ 3. Статистика запросов                               ║
║                                                      ║
║ 0. Выход                                             ║
║                                                      ║
╚══════════════════════════════════════════════════════╝
```

Пример вывода погоды:
```
┌──────────────────────────────────────────────┐
│ МОСКВА                                ☀️     │
├──────────────────────────────────────────────┤
│ Температура:  24.3°C (ощущается как 26.1°C)  │
│ Влажность:              65%                  │
│ Ветер:                  3.2 м/с              │
│ Описание:     ясно                           │
└──────────────────────────────────────────────┘

╔══════════════════════════════════════════════╗
║ ☀️ Легкая одежда, головной убор от солнца    ║
║ 🌂 Зонт не нужен                             ║
║ 🧴 Используйте солнцезащитный крем           ║
╚══════════════════════════════════════════════╝
```

## �🛠 Техническая информация

### Архитектура
- **Модели**: `WeatherData`, `MainData`, `WeatherDescription`
- **Сервисы**:
  - `WeatherApiClient` - работа с API
  - `WeatherFacade` - основной функционал
  - `WeatherLogger` - логирование (Singleton)
  - `ConfigurationService` - управление настройками

### Файловая структура
```
WeatherTerminal/
├── Models/
│   ├── WeatherData.cs
│   └── LogLevel.cs
├── Services/
│   ├── WeatherApiClient.cs
│   ├── WeatherFacade.cs
│   ├── WeatherLogger.cs
│   └── ConfigurationService.cs
├── Program.cs
└── appsettings.json
```

## 📊 Логирование
Приложение создает:
- Ежедневные логи в `/WeatherLogs/`
- Кеш данных в `weather_cache.json`
- Статистику запросов в `api_key_log.txt`

## 📜 Лицензия
MIT License. Подробнее в файле [LICENSE](LICENSE).
