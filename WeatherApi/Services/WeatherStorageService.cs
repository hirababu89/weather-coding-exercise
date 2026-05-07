using System.Text.Json;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class WeatherStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<WeatherStorageService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public WeatherStorageService(IConfiguration config, ILogger<WeatherStorageService> logger)
    {
        _logger = logger;

        // Resolve relative to the content root so it works regardless of CWD
        var contentRoot = config["ContentRootPath"] ?? Directory.GetCurrentDirectory();
        _storageRoot = Path.Combine(contentRoot, "weather-data");
        Directory.CreateDirectory(_storageRoot);
    }

    public bool TryLoad(string isoDate, out WeatherEntry? entry)
    {
        var path = FilePath(isoDate);
        if (!File.Exists(path))
        {
            entry = null;
            return false;
        }

        try
        {
            var json = File.ReadAllText(path);
            entry = JsonSerializer.Deserialize<WeatherEntry>(json, JsonOpts);
            _logger.LogInformation("Cache hit for {Date}", isoDate);
            return entry is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cached entry for {Date}; will re-fetch", isoDate);
            entry = null;
            return false;
        }
    }

    public void Save(string isoDate, WeatherEntry entry)
    {
        var path = FilePath(isoDate);
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(entry, JsonOpts));
            _logger.LogInformation("Saved weather data for {Date}", isoDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save weather data for {Date}", isoDate);
        }
    }

    private string FilePath(string isoDate) => Path.Combine(_storageRoot, $"{isoDate}.json");
}
