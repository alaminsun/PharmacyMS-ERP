import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'warning' | 'info';
export interface ToastMessage {
  id: number;
  kind: ToastKind;
  message: string;
  durationMs: number;
}

/** A lightweight Angular 21 notification service; no external toastr dependency. */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
  private readonly items = signal<ToastMessage[]>([]);
  readonly toasts = this.items.asReadonly();

  success(message: string): void { this.show('success', message, 4500); }
  error(message: string): void { this.show('error', message, 7500); }
  warning(message: string): void { this.show('warning', message, 6000); }
  info(message: string): void { this.show('info', message, 4500); }

  private show(kind: ToastKind, message: string, durationMs: number): void {
    const value = message.trim();
    if (!value) return;
    // Coalesce repeated API failures; never suppress distinct successful user actions.
    if (kind === 'error' && this.items().some(item => item.kind === kind && item.message === value)) return;
    const id = ++this.nextId;
    const current = this.items();
    if (current.length >= 4) this.dismiss(current[0].id);
    this.items.update(items => [...items, { id, kind, message: value, durationMs }]);
    this.timers.set(id, setTimeout(() => this.dismiss(id), durationMs));
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer !== undefined) clearTimeout(timer);
    this.timers.delete(id);
    this.items.update(items => items.filter(item => item.id !== id));
  }
}
