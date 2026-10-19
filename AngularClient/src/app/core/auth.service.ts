import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { LoginResponse, MyAccess } from './models';

/** Tokens are session-scoped (sessionStorage), not persistent localStorage. The API rechecks DB permissions. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);
  private readonly tokenKey = 'pharmacyms.accessToken';
  private readonly expiryKey = 'pharmacyms.expiresAt';
  readonly access = signal<MyAccess | null>(null);
  readonly initializing = signal(true);
  token(): string | null {
    const token = sessionStorage.getItem(this.tokenKey);
    const expires = sessionStorage.getItem(this.expiryKey);
    if (!token || !expires || Date.now() >= Date.parse(expires)) return null;
    return token;
  }
  authenticated() { return Boolean(this.token()) && Boolean(this.access()); }
  can(permission: string): boolean { return this.access()?.permissions.includes(permission) ?? false; }
  async initialize(): Promise<void> {
    try { if (this.token()) await this.reloadAccess(); else this.clear(); }
    catch { this.clear(); }
    finally { this.initializing.set(false); }
  }
  async login(email: string, password: string): Promise<void> {
    const result = await this.api.post<LoginResponse>('/auth/login', { email, password });
    sessionStorage.setItem(this.tokenKey, result.accessToken);
    sessionStorage.setItem(this.expiryKey, result.expiresAtUtc);
    try { await this.reloadAccess(); }
    catch (error) { this.clear(); throw error; }
  }
  async reloadAccess(): Promise<void> { this.access.set(await this.api.get<MyAccess>('/auth/me/permissions')); }
  clear() { sessionStorage.removeItem(this.tokenKey); sessionStorage.removeItem(this.expiryKey); this.access.set(null); }
  logout() { this.clear(); void this.router.navigateByUrl('/login'); }
}
