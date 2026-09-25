import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
import { API_BASE_URL } from './core/api/api-base-url';
import { authInterceptor } from './core/auth/auth.interceptor';
import { sessionExpiredInterceptor } from './core/auth/session-expired.interceptor';

// Matches the "http" profile of DailyVibe.Api; the API only allows CORS from http://localhost:4200.
const DEV_API_BASE_URL = 'http://localhost:5208';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor, sessionExpiredInterceptor])),
    { provide: API_BASE_URL, useValue: DEV_API_BASE_URL },
  ],
};
