import { TestBed } from '@angular/core/testing';
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';

import { sessionExpiredInterceptor } from './session-expired.interceptor';
import { TokenStorage } from './token-storage';

const URL = 'http://api.test/api/messages/today';
const AUTHENTICATED = { headers: new HttpHeaders({ Authorization: 'Bearer old' }) };

describe('sessionExpiredInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let storage: TokenStorage;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([sessionExpiredInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    storage = TestBed.inject(TokenStorage);
    router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);
    storage.save('old');
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  const request = (options = AUTHENTICATED) => {
    let error: HttpErrorResponse | undefined;
    client.get(URL, options).subscribe({ error: (e: HttpErrorResponse) => (error = e) });
    return () => error;
  };

  it('drops the token and goes to /login when an authenticated request gets 401', () => {
    const error = request();

    http.expectOne(URL).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(storage.read()).toBeNull();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
    expect(error()?.status).toBe(401);
  });

  it('keeps the session on errors other than 401', () => {
    const error = request();

    http.expectOne(URL).flush(null, { status: 503, statusText: 'Service Unavailable' });

    expect(storage.read()).toBe('old');
    expect(router.navigateByUrl).not.toHaveBeenCalled();
    expect(error()?.status).toBe(503);
  });

  it('ignores a 401 on an anonymous request such as a failed login', () => {
    request({ headers: new HttpHeaders() });

    http.expectOne(URL).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(storage.read()).toBe('old');
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});
