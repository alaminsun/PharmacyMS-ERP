import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { ConfirmOutletComponent } from './shared/confirm-outlet.component';
import { LoadingService } from './core/loading.service';
import { ToastOutletComponent } from './shared/toast-outlet.component';
@Component({selector:'app-root',standalone:true,imports:[RouterOutlet,ToastOutletComponent,ConfirmOutletComponent],template:`@if(loading.busy()){<div class="global-progress" role="progressbar" aria-label="Loading"></div>}<router-outlet/><app-toast-outlet/><app-confirm-outlet/>`})
export class AppComponent implements OnInit {
  private auth = inject(AuthService);
  readonly loading=inject(LoadingService);
  ngOnInit() { void this.auth.initialize(); }
}
