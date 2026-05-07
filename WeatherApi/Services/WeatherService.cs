using WeatherApi.Models;

namespace WeatherApi.Services;

public class WeatherService
{
    private readonly OpenMeteoClient _meteoClient;
    private readonly WeatherStorageService _storage;
    private readonly IConfiguration _config;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        OpenMeteoClient meteoClient,
        WeatherStorageService storage,
        IConfiguration config,
        ILogger<WeatherService> logger)
    {
        _meteoClient = meteoClient;
        _storage = storage;
        _config = config;
        _logger = logger;
    }

    public async Task<WeatherResponse> GetAllAsync(CancellationToken ct = default)
    {
        var dates = ReadDatesFile();
        var tasks = dates.Select(raw => ProcessDateAsync(raw, ct));
        var results = await Task.WhenAll(tasks);
        return new WeatherResponse(results);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private IEnumerable<string> ReadDatesFile()
    {
        var contentRoot = _config["ContentRootPath"] ?? Directory.GetCurrentDirectory();
        var path = Path.Combine(contentRoot, "dates.txt");

        if (!File.Exists(path))
        {
            _logger.LogError("dates.txt not found at {Path}", path);
            return [];
        }

        return File.ReadAllLines(path)
                   .Select(l => l.Trim())
                   .Where(l => l.Length > 0);
    }

    private async Task<WeatherEntry> ProcessDateAsync(string raw, CancellationToken ct)
    {
        // 1. Parse
        var (isoDate, parseError) = DateParser.TryParse(raw);
        if (isoDate is null)
        {
            _logger.LogWarning("Invalid date '{Raw}': {Error}", raw, parseError);
            return new WeatherEntry(raw, null, null, null, null, "InvalidDate", parseError);
        }

        // 2. Cache hit?
        if (_storage.TryLoad(isoDate, out var cached) && cached is not null)
            return cached;

        // 3. Fetch from API
        var (minTemp, maxTemp, precip, fetchError) = await _meteoClient.FetchDailyAsync(isoDate, ct);

        WeatherEntry entry;
        if (fetchError is not null)
        {
            entry = new WeatherEntry(raw, isoDate, null, null, null, "FetchError", fetchError);
        }
        else if (minTemp is null && maxTemp is null && precip is null)
        {
            entry = new WeatherEntry(raw, isoDate, null, null, null, "NoData", "All fields were null in API response");
        }
        else
        {
            entry = new WeatherEntry(raw, isoDate, minTemp, maxTemp, precip, "OK", null);
            _storage.Save(isoDate, entry);
        }

        return entry;
    }
}
