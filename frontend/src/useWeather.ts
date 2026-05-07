import { useState, useEffect } from 'react';
import type { WeatherResponse } from './types';

const API_BASE = import.meta.env.VITE_API_URL ?? '';

interface UseWeatherResult {
  data: WeatherResponse | null;
  loading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useWeather(): UseWeatherResult {
  const [data, setData] = useState<WeatherResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    fetch(`${API_BASE}/api/weather`)
      .then(res => {
        if (!res.ok) throw new Error(`Server responded with ${res.status} ${res.statusText}`);
        return res.json() as Promise<WeatherResponse>;
      })
      .then(json => {
        if (!cancelled) setData(json);
      })
      .catch((err: Error) => {
        if (!cancelled) setError(err.message ?? 'Unknown error');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [tick]);

  return { data, loading, error, refetch: () => setTick(t => t + 1) };
}
