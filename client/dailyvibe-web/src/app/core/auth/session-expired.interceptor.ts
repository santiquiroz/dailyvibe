import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
  HttpStatusCode,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { TokenStorage } from './token-storage';

export const sessionExpiredInterceptor: HttpInterceptorFn = (request, next) => {
  const storage = inject(TokenStorage);
  const router = inject(Router);
  return next(request).pipe(
    tap({
      error: (error: unknown) => {
        if (isRejectedToken(request, error)) {
          storage.clear();
          void router.navigateByUrl('/login');
        }
      },
    }),
  );
};

function isRejectedToken(request: HttpRequest<unknown>, error: unknown): boolean {
  return (
    error instanceof HttpErrorResponse &&
    error.status === HttpStatusCode.Unauthorized &&
    request.headers.has('Authorization')
  );
}
