import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { errorMessage } from './api.service';
import { ToastService } from './toast.service';

/** Show all API errors, including failed reads, without duplicating component catch blocks. */
export const toastErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const toast = inject(ToastService);
  return next(request).pipe(catchError((error: unknown) => {
    if (error instanceof HttpErrorResponse && request.url.startsWith('/api/')) {
      toast.error(errorMessage(error));
    }
    return throwError(() => error);
  }));
};
