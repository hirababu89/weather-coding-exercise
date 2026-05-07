using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Expose content root path to services that need it for file I/O
builder.Configuration["ContentRootPath"] = builder.Environment.ContentRootPath;

builder.Services.AddControllers();

// Named CORS policy for local frontend dev
builder.Services.AddCors(opt =>
    opt.AddPolicy("LocalFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()));

// Register typed HttpClient for Open-Meteo (respects HttpClientFactory socket pooling)
builder.Services.AddHttpClient<OpenMeteoClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<WeatherStorageService>();
builder.Services.AddScoped<WeatherService>();

var app = builder.Build();

app.UseCors("LocalFrontend");
app.MapControllers();

app.Run();
