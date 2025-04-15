using System.Text.Json.Serialization;

namespace WeatherTerminal.Models;

/// <summary>
/// Represents complete weather data for a specific location
/// </summary>
public sealed class WeatherData
{
    /// <summary>
    /// City name
    /// </summary>
    [JsonPropertyName("name")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Main weather parameters (temperature, humidity, etc.)
    /// </summary>
    [JsonPropertyName("main")]
    public MainData Main { get; set; } = new();

    /// <summary>
    /// List of weather conditions (usually contains one element)
    /// </summary>
    [JsonPropertyName("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();

    /// <summary>
    /// Wind parameters
    /// </summary>
    [JsonPropertyName("wind")]
    public WindData Wind { get; set; } = new();

    /// <summary>
    /// Primary weather description (first from the list if available)
    /// </summary>
    public string Description => Weather?.FirstOrDefault()?.Description ?? "N/A";

    /// <summary>
    /// Weather icon code (first from the list if available)
    /// </summary>
    public string Icon => Weather?.FirstOrDefault()?.Icon ?? string.Empty;
}

/// <summary>
/// Contains main meteorological data
/// </summary>
public sealed class MainData
{
    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }

    /// <summary>
    /// Human perception of weather temperature in Celsius
    /// </summary>
    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    /// <summary>
    /// Humidity percentage (0-100)
    /// </summary>
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

/// <summary>
/// Weather condition description
/// </summary>
public sealed class WeatherDescription
{
    /// <summary>
    /// Textual description of weather conditions
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Weather icon identifier for visual representation
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Wind parameters
/// </summary>
public sealed class WindData
{
    /// <summary>
    /// Wind speed in meters per second
    /// </summary>
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}

