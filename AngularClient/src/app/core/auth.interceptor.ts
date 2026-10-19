import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  // Never forward the bearer token to unrelated origins.
  const token = auth.token();
  const withAuth = token && request.url.startsWith('/api/')
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
  return next(withAuth).pipe(catchError((error: unknown) => {
    if (error instanceof HttpErrorResponse && error.status === 401 && !request.url.endsWith('/auth/login')) auth.logout();
    return throwError(() => error);
  }));
};
