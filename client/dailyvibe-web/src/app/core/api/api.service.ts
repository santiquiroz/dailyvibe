import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from './api-base-url';
import { AuthResult, Credentials, DailyMessage, PagedResult } from './api.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  register(credentials: Credentials): Observable<AuthResult> {
    return this.http.post<AuthResult>(this.url('auth/register'), credentials);
  }

  login(credentials: Credentials): Observable<AuthResult> {
    return this.http.post<AuthResult>(this.url('auth/login'), credentials);
  }

  getToday(): Observable<DailyMessage> {
    return this.http.get<DailyMessage>(this.url('messages/today'));
  }

  generate(intent: string | null = null): Observable<DailyMessage> {
    return this.http.post<DailyMessage>(this.url('messages/generate'), { intent });
  }

  getHistory(page: number, size: number): Observable<PagedResult<DailyMessage>> {
    return this.http.get<PagedResult<DailyMessage>>(this.url('messages/history'), {
      params: { page, size },
    });
  }

  updatePreferences(defaultIntent: string): Observable<void> {
    return this.http.put<void>(this.url('preferences'), { defaultIntent });
  }

  private url(path: string): string {
    return `${this.baseUrl}/api/${path}`;
  }
}
