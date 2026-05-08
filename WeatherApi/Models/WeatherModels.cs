using System.Text.Json.Serialization;

namespace WeatherApi.Models;

public record WeatherEntry(
    string RawInput,
    string? NormalizedDate,
    double? MinTemperatureCelsius,
    double? MaxTemperatureCelsius,
    double? PrecipitationMm,
    string Status,
    string? ErrorMessage
);

public record WeatherResponse(IReadOnlyList<WeatherEntry> Results);

// Open-Meteo API response shape
public record OpenMeteoResponse(
    [property: JsonPropertyName("daily")] OpenMeteoDailyData? Daily
);

public record OpenMeteoDailyData(
    [property: JsonPropertyName("time")] IList<string>? Time,
    [property: JsonPropertyName("temperature_2m_max")] IList<double?>? Temperature2mMax,
    [property: JsonPropertyName("temperature_2m_min")] IList<double?>? Temperature2mMin,
    [property: JsonPropertyName("precipitation_sum")] IList<double?>? PrecipitationSum
);
