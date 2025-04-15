// ConfigurationService.cs
using System.Text.Json;

namespace WeatherTerminal.Services;

public sealed class ConfigurationService
{
    private const string ConfigFile = "appsettings.json";
    private readonly Dictionary<string, string> _config;

    public ConfigurationService()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                _config = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            else
            {
                _config = new();
                // Создаем файл с дефолтными настройками
                SaveConfig();
            }
        }
        catch
        {
            _config = new();
        }
    }

    public string? GetApiKey() => _config.TryGetValue("ApiKey", out var key) ? key : null;

    public void SetApiKey(string key)
    {
        _config["ApiKey"] = key;
        SaveConfig();
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }
        catch { /* ignore */ }
    }
}
