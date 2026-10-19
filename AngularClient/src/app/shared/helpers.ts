export function display(value: unknown): string {
  if (value === null || value === undefined || value === '') return '—';
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (typeof value === 'number') return value.toLocaleString(undefined, { maximumFractionDigits: 2 });
  return String(value);
}
export function money(value: unknown): string { return Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
export function humanize(key: string): string { return key.replace(/([a-z])([A-Z])/g,'$1 $2').replace(/^./, s=>s.toUpperCase()); }
export function isoStart(date: string): string { return `${date}T00:00:00Z`; }
export function isoEndExclusive(date: string): string { const value = new Date(`${date}T00:00:00Z`); value.setUTCDate(value.getUTCDate() + 1); return value.toISOString(); }
export function todayLocal(): string { const now = new Date(); const y = now.getFullYear(); const m = String(now.getMonth()+1).padStart(2, '0'); const d = String(now.getDate()).padStart(2,'0'); return `${y}-${m}-${d}`; }
export function safeCsv(rows: Record<string, unknown>[], columns: string[]): string {
  const escape = (value: unknown) => {
    let text = String(value ?? '');
    // Prevent spreadsheet formula injection when exported data is opened in Excel.
    if (/^[\s\uFEFF]*[=+@\-\t\r]/.test(text)) text = `'${text}`;
    return `"${text.replace(/"/g,'""')}"`;
  };
  return `\uFEFF${columns.map(escape).join(',')}\r\n${rows.map(row => columns.map(key=>escape(row[key])).join(',')).join('\r\n')}`;
}
export function downloadCsv(rows: Record<string, unknown>[], columns: string[], filename: string) {
  const blob = new Blob([safeCsv(rows,columns)], { type:'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a'); anchor.href = url; anchor.download = filename; anchor.click();
  URL.revokeObjectURL(url);
}
