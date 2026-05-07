import { useState, useMemo } from 'react';
import { useWeather } from './useWeather';
import type { WeatherEntry, SortField, SortDir } from './types';

// ─── Helpers ────────────────────────────────────────────────────────────────

function fmt(val: number | null, unit: string): string {
  return val !== null ? `${val.toFixed(1)} ${unit}` : '—';
}

function statusBadge(status: string) {
  const map: Record<string, string> = {
    OK: 'badge-ok',
    InvalidDate: 'badge-invalid',
    FetchError: 'badge-error',
    NoData: 'badge-nodata',
  };
  return map[status] ?? 'badge-nodata';
}

function sortEntries(
  entries: WeatherEntry[],
  field: SortField,
  dir: SortDir
): WeatherEntry[] {
  return [...entries].sort((a, b) => {
    let av: number | string | null;
    let bv: number | string | null;
    switch (field) {
      case 'date':   av = a.normalizedDate ?? ''; bv = b.normalizedDate ?? ''; break;
      case 'minTemp': av = a.minTemperatureCelsius; bv = b.minTemperatureCelsius; break;
      case 'maxTemp': av = a.maxTemperatureCelsius; bv = b.maxTemperatureCelsius; break;
      case 'precip':  av = a.precipitationMm;      bv = b.precipitationMm;      break;
    }
    // Nulls sink to bottom regardless of sort direction
    if (av === null) return 1;
    if (bv === null) return -1;
    const cmp = av < bv ? -1 : av > bv ? 1 : 0;
    return dir === 'asc' ? cmp : -cmp;
  });
}

// ─── Sub-components ──────────────────────────────────────────────────────────

function SortIcon({ active, dir }: { active: boolean; dir: SortDir }) {
  if (!active) return <span className="sort-icon inactive">⇅</span>;
  return <span className="sort-icon active">{dir === 'asc' ? '↑' : '↓'}</span>;
}

function DetailPanel({ entry, onClose }: { entry: WeatherEntry; onClose: () => void }) {
  return (
    <div className="detail-overlay" onClick={onClose}>
      <div className="detail-card" onClick={e => e.stopPropagation()}>
        <button className="detail-close" onClick={onClose}>✕</button>
        <h2 className="detail-title">{entry.normalizedDate ?? entry.rawInput}</h2>
        <p className="detail-raw">Raw input: <code>{entry.rawInput}</code></p>
        <div className="detail-grid">
          <div className="detail-item">
            <span className="detail-label">Min Temp</span>
            <span className="detail-value">{fmt(entry.minTemperatureCelsius, '°C')}</span>
          </div>
          <div className="detail-item">
            <span className="detail-label">Max Temp</span>
            <span className="detail-value">{fmt(entry.maxTemperatureCelsius, '°C')}</span>
          </div>
          <div className="detail-item">
            <span className="detail-label">Precipitation</span>
            <span className="detail-value">{fmt(entry.precipitationMm, 'mm')}</span>
          </div>
          <div className="detail-item">
            <span className="detail-label">Status</span>
            <span className={`badge ${statusBadge(entry.status)}`}>{entry.status}</span>
          </div>
        </div>
        {entry.errorMessage && (
          <p className="detail-error">⚠ {entry.errorMessage}</p>
        )}
      </div>
    </div>
  );
}

// ─── App ─────────────────────────────────────────────────────────────────────

export default function App() {
  const { data, loading, error, refetch } = useWeather();
  const [sortField, setSortField] = useState<SortField>('date');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [selected, setSelected] = useState<WeatherEntry | null>(null);

  const sorted = useMemo(
    () => (data ? sortEntries(data.results, sortField, sortDir) : []),
    [data, sortField, sortDir]
  );

  function handleSort(field: SortField) {
    if (field === sortField) {
      setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDir('asc');
    }
  }

  function colHeader(label: string, field: SortField) {
    return (
      <th className="sortable" onClick={() => handleSort(field)}>
        {label}
        <SortIcon active={sortField === field} dir={sortDir} />
      </th>
    );
  }

  return (
    <>
      <header className="app-header">
        <div className="header-inner">
          <div className="header-title">
            <span className="header-icon">⛅</span>
            <h1>Dallas Historical Weather</h1>
          </div>
          <p className="header-sub">Open-Meteo · Dallas, TX (32.78°N, 96.80°W)</p>
        </div>
      </header>

      <main className="main">
        {loading && (
          <div className="state-container">
            <div className="spinner" />
            <p className="state-text">Fetching weather data…</p>
          </div>
        )}

        {error && !loading && (
          <div className="state-container error-state">
            <span className="state-icon">⚠</span>
            <p className="state-text">Failed to load data</p>
            <p className="state-detail">{error}</p>
            <button className="btn-retry" onClick={refetch}>Retry</button>
          </div>
        )}

        {data && !loading && (
          <>
            <div className="table-meta">
              <span>{sorted.length} date{sorted.length !== 1 ? 's' : ''}</span>
              <span className="table-hint">Click a row for details · Click a column header to sort</span>
            </div>
            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    {colHeader('Date', 'date')}
                    <th>Raw Input</th>
                    {colHeader('Min Temp', 'minTemp')}
                    {colHeader('Max Temp', 'maxTemp')}
                    {colHeader('Precipitation', 'precip')}
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {sorted.map((entry, i) => (
                    <tr
                      key={i}
                      className={entry.status !== 'OK' ? 'row-error' : ''}
                      onClick={() => setSelected(entry)}
                      tabIndex={0}
                      onKeyDown={e => e.key === 'Enter' && setSelected(entry)}
                    >
                      <td className="cell-date">{entry.normalizedDate ?? '—'}</td>
                      <td className="cell-raw"><code>{entry.rawInput}</code></td>
                      <td>{fmt(entry.minTemperatureCelsius, '°C')}</td>
                      <td>{fmt(entry.maxTemperatureCelsius, '°C')}</td>
                      <td>{fmt(entry.precipitationMm, 'mm')}</td>
                      <td>
                        <span className={`badge ${statusBadge(entry.status)}`}>
                          {entry.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </main>

      {selected && (
        <DetailPanel entry={selected} onClose={() => setSelected(null)} />
      )}
    </>
  );
}
