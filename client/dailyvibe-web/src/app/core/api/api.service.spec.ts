import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { API_BASE_URL } from './api-base-url';
import { ApiService } from './api.service';
import { AuthResult, DailyMessage, PagedResult } from './api.models';

const BASE_URL = 'http://api.test';

const AUTH_RESULT: AuthResult = { userId: 'u-1', email: 'ana@example.com', token: 'jwt' };
const MESSAGE: DailyMessage = {
  id: 'm-1',
  content: 'Hoy es un buen día.',
  intent: 'estoico',
  createdAt: '2026-09-25T10:00:00Z',
};

describe('ApiService', () => {
  let api: ApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    api = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('registers with POST /api/auth/register and returns the auth result', () => {
    let result: AuthResult | undefined;
    api
      .register({ email: 'ana@example.com', password: 'secreto123' })
      .subscribe((r) => (result = r));

    const req = http.expectOne(`${BASE_URL}/api/auth/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'ana@example.com', password: 'secreto123' });
    req.flush(AUTH_RESULT);

    expect(result).toEqual(AUTH_RESULT);
  });

  it('logs in with POST /api/auth/login', () => {
    api.login({ email: 'ana@example.com', password: 'secreto123' }).subscribe();

    const req = http.expectOne(`${BASE_URL}/api/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'ana@example.com', password: 'secreto123' });
    req.flush(AUTH_RESULT);
  });

  it("gets today's message with GET /api/messages/today", () => {
    let result: DailyMessage | undefined;
    api.getToday().subscribe((m) => (result = m));

    const req = http.expectOne(`${BASE_URL}/api/messages/today`);
    expect(req.request.method).toBe('GET');
    req.flush(MESSAGE);

    expect(result).toEqual(MESSAGE);
  });

  it('generates with POST /api/messages/generate sending the intent', () => {
    api.generate('humorístico').subscribe();

    const req = http.expectOne(`${BASE_URL}/api/messages/generate`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ intent: 'humorístico' });
    req.flush(MESSAGE);
  });

  it('generates with a null intent so the API falls back to the default one', () => {
    api.generate().subscribe();

    const req = http.expectOne(`${BASE_URL}/api/messages/generate`);
    expect(req.request.body).toEqual({ intent: null });
    req.flush(MESSAGE);
  });

  it('gets the history with GET /api/messages/history?page&size', () => {
    const page: PagedResult<DailyMessage> = { items: [MESSAGE], page: 2, size: 5, totalCount: 6 };
    let result: PagedResult<DailyMessage> | undefined;
    api.getHistory(2, 5).subscribe((p) => (result = p));

    const req = http.expectOne(
      (r) => r.url === `${BASE_URL}/api/messages/history` && r.method === 'GET',
    );
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('size')).toBe('5');
    req.flush(page);

    expect(result).toEqual(page);
  });

  it('updates preferences with PUT /api/preferences', () => {
    let completed = false;
    api.updatePreferences('reflexivo').subscribe({ complete: () => (completed = true) });

    const req = http.expectOne(`${BASE_URL}/api/preferences`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ defaultIntent: 'reflexivo' });
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBeTrue();
  });
});
