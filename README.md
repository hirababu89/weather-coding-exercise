# Weather Coding Exercise

Historical weather viewer for Dallas, TX using the [Open-Meteo Historical API](https://open-meteo.com/en/docs/historical-weather-api).

## Architecture

```
weather-coding-exercise/
├── WeatherApi/          # .NET 8 Web API (C#)
│   ├── Controllers/     # WeatherController → GET /api/weather
│   ├── Services/        # DateParser, OpenMeteoClient, WeatherStorageService, WeatherService
│   ├── Models/          # WeatherEntry, WeatherResponse, OpenMeteoResponse
│   ├── dates.txt        # Input dates (multiple formats)
│   └── weather-data/    # Per-date JSON cache (git-ignored)
└── frontend/            # React 18 + TypeScript + Vite
    └── src/
        ├── App.tsx       # Table with sort + row-detail interaction
        ├── useWeather.ts # Data-fetching hook
        └── index.css     # Dark dashboard styling
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)

## Running the Backend

```bash
cd WeatherApi
dotnet run
# Listens on http://localhost:5000
```

The backend will:
1. Read `dates.txt` from the project root
2. Parse each line (supports `MM/dd/yyyy`, `MMMM d, yyyy`, `MMM-dd-yyyy`)
3. Reject invalid calendar dates (e.g. April 31) without crashing
4. Fetch daily weather from Open-Meteo for each valid date (Dallas, TX)
5. Cache results as JSON under `weather-data/` — skips repeat API calls

### API

```
GET http://localhost:5000/api/weather
```

Response shape:
```json
{
  "results": [
    {
      "rawInput": "02/27/2021",
      "normalizedDate": "2021-02-27",
      "minTemperatureCelsius": -12.3,
      "maxTemperatureCelsius": -4.1,
      "precipitationMm": 2.5,
      "status": "OK",
      "errorMessage": null
    },
    {
      "rawInput": "April 31, 2022",
      "normalizedDate": null,
      "minTemperatureCelsius": null,
      "maxTemperatureCelsius": null,
      "precipitationMm": null,
      "status": "InvalidDate",
      "errorMessage": "Unrecognized or invalid date: 'April 31, 2022'"
    }
  ]
}
```

**Status values:**
| Status | Meaning |
|---|---|
| `OK` | Fetched successfully |
| `InvalidDate` | Failed date parsing (e.g. April 31) |
| `FetchError` | Network or API error |
| `NoData` | API returned empty result |

## Running the Frontend

```bash
cd frontend
npm install
npm run dev
# Opens http://localhost:5173
```

The Vite dev server proxies `/api/*` to `http://localhost:5000`, so both services need to be running.

## Features

- Multi-format date parsing with graceful invalid-date handling
- Local JSON caching avoids redundant API calls
- Sortable columns (date, min temp, max temp, precipitation)
- Click any row to see a detail modal
- Loading spinner and error state with retry button

## Assumptions

- Coordinates fixed to Dallas, TX (32.78°N, 96.80°W)
- Temperatures returned by Open-Meteo are in Celsius (`temperature_unit` default)
- `April 31, 2022` is treated as invalid — April has 30 days
- The `weather-data/` folder is excluded from version control (no stale data shipped with the repo)
- No authentication is needed for Open-Meteo (free, non-commercial use)
