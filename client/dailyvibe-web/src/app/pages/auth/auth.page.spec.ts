import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../core/api/api-base-url';
import { TokenStorage } from '../../core/auth/token-storage';
import { AuthMode, AuthPage } from './auth.page';

const BASE_URL = 'http://api.test';

describe('AuthPage', () => {
  let fixture: ComponentFixture<AuthPage>;
  let http: HttpTestingController;
  let router: Router;

  const setUp = async (mode: AuthMode) => {
    fixture = TestBed.createComponent(AuthPage);
    fixture.componentRef.setInput('mode', mode);
    await fixture.whenStable();
  };

  const element = () => fixture.nativeElement as HTMLElement;

  const fillAndSubmit = async (email: string, password: string) => {
    const [emailInput, passwordInput] = Array.from(element().querySelectorAll('input'));
    emailInput.value = email;
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = password;
    passwordInput.dispatchEvent(new Event('input'));
    element().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [AuthPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: BASE_URL },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('logs in, stores the token and goes home', async () => {
    await setUp('login');

    await fillAndSubmit('ana@example.com', 'secreto123');
    http
      .expectOne(`${BASE_URL}/api/auth/login`)
      .flush({ userId: 'u-1', email: 'ana@example.com', token: 'jwt-login' });

    expect(TestBed.inject(TokenStorage).read()).toBe('jwt-login');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('registers through /api/auth/register in register mode', async () => {
    await setUp('register');

    await fillAndSubmit('ana@example.com', 'secreto123');
    http
      .expectOne(`${BASE_URL}/api/auth/register`)
      .flush({ userId: 'u-1', email: 'ana@example.com', token: 'jwt-register' });

    expect(TestBed.inject(TokenStorage).read()).toBe('jwt-register');
  });

  it('does not call the API while the form is invalid', async () => {
    await setUp('register');

    await fillAndSubmit('no-es-un-email', 'corta');

    http.expectNone(() => true);
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('shows the API error and stays on the page', async () => {
    await setUp('login');

    await fillAndSubmit('ana@example.com', 'secreto123');
    http
      .expectOne(`${BASE_URL}/api/auth/login`)
      .flush(
        { status: 401, detail: 'Credenciales inválidas.' },
        { status: 401, statusText: 'Unauthorized' },
      );
    await fixture.whenStable();

    expect(element().querySelector('[role="alert"]')?.textContent).toContain(
      'Credenciales inválidas.',
    );
    expect(TestBed.inject(TokenStorage).read()).toBeNull();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});
