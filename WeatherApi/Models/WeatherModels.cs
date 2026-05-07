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
    OpenMeteoDailyData? Daily
);

public record OpenMeteoDailyData(
    IList<string>? Time,
    IList<double?>? Temperature2mMax,
    IList<double?>? Temperature2mMin,
    IList<double?>? PrecipitationSum
);
