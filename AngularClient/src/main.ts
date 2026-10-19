import { provideZoneChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { AppComponent } from './app/app.component';
import { appRoutes } from './app/app.routes';
import { authInterceptor } from './app/core/auth.interceptor';
import { loadingInterceptor } from './app/core/loading.interceptor';
import { toastErrorInterceptor } from './app/core/toast.interceptor';

bootstrapApplication(AppComponent, {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }), provideHttpClient(withInterceptors([authInterceptor, loadingInterceptor, toastErrorInterceptor])), provideRouter(appRoutes, withComponentInputBinding())]
}).catch(error => console.error('Unable to start PharmacyMS Angular Client', error));
