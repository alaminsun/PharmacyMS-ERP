import { Component, inject } from '@angular/core';
import { ToastService } from '../core/toast.service';

@Component({
  selector: 'app-toast-outlet',
  standalone: true,
  template: `
    <div class="toast-stack" aria-label="Notifications">
      @for (toast of notifications.toasts(); track toast.id) {
        <div class="app-toast" [class.toast-success]="toast.kind === 'success'"
             [class.toast-error]="toast.kind === 'error'"
             [class.toast-warning]="toast.kind === 'warning'"
             [class.toast-info]="toast.kind === 'info'"
             [attr.role]="toast.kind === 'error' ? 'alert' : 'status'"
             [attr.aria-live]="toast.kind === 'error' ? 'assertive' : 'polite'">
          <span class="toast-icon" aria-hidden="true">{{toast.kind === 'success' ? '✓' : toast.kind === 'error' ? '!' : toast.kind === 'warning' ? '⚠' : 'i'}}</span>
          <div class="toast-message"><strong>{{heading(toast.kind)}}</strong><span>{{toast.message}}</span></div>
          <button type="button" class="toast-close" aria-label="Dismiss notification"
                  (click)="notifications.dismiss(toast.id)">×</button>
          <span class="toast-progress" [style.animation-duration.ms]="toast.durationMs" aria-hidden="true"></span>
        </div>
      }
    </div>
  `
})
export class ToastOutletComponent {
  readonly notifications = inject(ToastService);
  heading(kind: string): string {
    return kind === 'success' ? 'Success' : kind === 'error' ? 'Something went wrong' : kind === 'warning' ? 'Please check' : 'Information';
  }
}
