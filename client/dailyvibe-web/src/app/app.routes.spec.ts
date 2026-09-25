import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';

import { API_BASE_URL } from './core/api/api-base-url';
import { routes } from './app.routes';
import { AuthPage } from './pages/auth/auth.page';

describe('routes', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(routes, withComponentInputBinding()),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'http://api.test' },
      ],
    });
  });

  afterEach(() => localStorage.clear());

  it('sends an anonymous visitor of the home page to /login', async () => {
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl('/');

    expect(TestBed.inject(Router).url).toBe('/login');
  });

  it('opens the auth page in register mode on /register', async () => {
    const harness = await RouterTestingHarness.create();

    const page = await harness.navigateByUrl('/register', AuthPage);

    expect(page.mode()).toBe('register');
  });

  it('redirects unknown paths to the home page (and so to /login when anonymous)', async () => {
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl('/no-existe');

    expect(TestBed.inject(Router).url).toBe('/login');
  });
});
