using System.Net.Http.Json;
using System.Text.Json;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class OpenMeteoClient
{
    // Dallas, TX coordinates
    private const double Latitude = 32.78;
    private const double Longitude = -96.80;
    private const string BaseUrl = "https://archive-api.open-meteo.com/v1/archive";

    private readonly HttpClient _http;
    private readonly ILogger<OpenMeteoClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public OpenMeteoClient(HttpClient http, ILogger<OpenMeteoClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Fetches daily weather for a single ISO date (yyyy-MM-dd).
    /// Returns null on failure; caller is responsible for building the error entry.
    /// </summary>
    public async Task<(double? minTemp, double? maxTemp, double? precip, string? error)>
        FetchDailyAsync(string isoDate, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}?latitude={Latitude}&longitude={Longitude}" +
                  $"&start_date={isoDate}&end_date={isoDate}" +
                  $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
                  $"&timezone=auto";

        try
        {
            _logger.LogInformation("Fetching Open-Meteo for {Date}", isoDate);

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var meteo = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(JsonOpts, ct);

            var daily = meteo?.Daily;
            if (daily is null || daily.Time is null || daily.Time.Count == 0)
                return (null, null, null, "API returned no data for this date");

            // Results are arrays indexed by day; we requested exactly one day.
            var maxTemp = daily.Temperature2mMax?.ElementAtOrDefault(0);
            var minTemp = daily.Temperature2mMin?.ElementAtOrDefault(0);
            var precip = daily.PrecipitationSum?.ElementAtOrDefault(0);

            return (minTemp, maxTemp, precip, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching weather for {Date}", isoDate);
            return (null, null, null, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather for {Date}", isoDate);
            return (null, null, null, $"Unexpected error: {ex.Message}");
        }
    }
}
