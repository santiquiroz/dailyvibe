import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { API_BASE_URL } from '../api/api-base-url';
import { authInterceptor } from './auth.interceptor';
import { TokenStorage } from './token-storage';

const BASE_URL = 'http://api.test';

describe('authInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let storage: TokenStorage;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    storage = TestBed.inject(TokenStorage);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('adds Authorization: Bearer <token> to API requests when a token is stored', () => {
    storage.save('abc.def.ghi');

    client.get(`${BASE_URL}/api/messages/today`).subscribe();

    const req = http.expectOne(`${BASE_URL}/api/messages/today`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc.def.ghi');
    req.flush({});
  });

  it('leaves the request untouched when there is no token', () => {
    client.get(`${BASE_URL}/api/messages/today`).subscribe();

    const req = http.expectOne(`${BASE_URL}/api/messages/today`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('never sends the token to a host other than the API', () => {
    storage.save('abc.def.ghi');

    client.get('https://other.example.com/data').subscribe();

    const req = http.expectOne('https://other.example.com/data');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });
});
