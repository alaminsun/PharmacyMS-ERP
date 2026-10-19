import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  get<T>(path: string, params?: Record<string, string | number | boolean | null | undefined>): Promise<T> {
    let query = new HttpParams();
    for (const [key, value] of Object.entries(params ?? {})) if (value !== null && value !== undefined) query = query.set(key, String(value));
    return firstValueFrom(this.http.get<T>(`/api${path}`, { params: query }));
  }
  post<T>(path: string, body: unknown): Promise<T> { return firstValueFrom(this.http.post<T>(`/api${path}`, body)); }
  put<T>(path: string, body: unknown): Promise<T> { return firstValueFrom(this.http.put<T>(`/api${path}`, body)); }
  delete<T>(path: string): Promise<T> { return firstValueFrom(this.http.delete<T>(`/api${path}`)); }
}
export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'Cannot reach the API. Run PharmacyMS.API and check proxy.conf.json.';
    if (error.status === 401) return 'Session expired or credentials are invalid. Please sign in again.';
    if (error.status === 403) return 'Access denied: this account lacks the required permission.';
    const data = error.error;
    const errors = data?.errors ? Object.values(data.errors as Record<string, string[]>).flat().join(' ') : '';
    return errors || data?.message || data?.detail || data?.title || `Request failed (HTTP ${error.status}).`;
  }
  return error instanceof Error ? error.message : 'An unexpected error occurred.';
}
