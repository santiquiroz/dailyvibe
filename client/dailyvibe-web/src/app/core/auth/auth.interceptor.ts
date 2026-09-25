import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_BASE_URL } from '../api/api-base-url';
import { TokenStorage } from './token-storage';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(TokenStorage).read();
  const isApiRequest = request.url.startsWith(`${inject(API_BASE_URL)}/`);
  if (!token || !isApiRequest) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
