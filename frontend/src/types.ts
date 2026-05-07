export type WeatherStatus = 'OK' | 'InvalidDate' | 'FetchError' | 'NoData';

export interface WeatherEntry {
  rawInput: string;
  normalizedDate: string | null;
  minTemperatureCelsius: number | null;
  maxTemperatureCelsius: number | null;
  precipitationMm: number | null;
  status: WeatherStatus;
  errorMessage: string | null;
}

export interface WeatherResponse {
  results: WeatherEntry[];
}

export type SortField = 'date' | 'minTemp' | 'maxTemp' | 'precip';
export type SortDir = 'asc' | 'desc';
