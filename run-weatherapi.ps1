$existing = Get-Process -Name WeatherApi -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Stopping existing WeatherApi process (PID $($existing.Id))"
    $existing | Stop-Process -Force
}

Set-Location $PSScriptRoot
Set-Location .\WeatherApi

dotnet run
