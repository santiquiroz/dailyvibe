import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  TestRequest,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../core/api/api-base-url';
import { DailyMessage, PagedResult } from '../../core/api/api.models';
import { TokenStorage } from '../../core/auth/token-storage';
import { HomePage } from './home.page';

const BASE_URL = 'http://api.test';
const TODAY_URL = `${BASE_URL}/api/messages/today`;
const HISTORY_URL = `${BASE_URL}/api/messages/history`;

const message = (id: string, content: string): DailyMessage => ({
  id,
  content,
  intent: 'estoico',
  createdAt: '2026-09-25T10:00:00Z',
});

const page = (items: DailyMessage[], pageNumber = 1, totalCount = items.length) =>
  ({ items, page: pageNumber, size: 10, totalCount }) satisfies PagedResult<DailyMessage>;

describe('HomePage', () => {
  let fixture: ComponentFixture<HomePage>;
  let http: HttpTestingController;
  let router: Router;

  const element = () => fixture.nativeElement as HTMLElement;
  const text = (selector: string) => element().querySelector(selector)?.textContent?.trim();
  const button = (label: string) =>
    Array.from(element().querySelectorAll('button')).find((b) => b.textContent?.trim() === label)!;
  const expectHistory = (pageNumber: number): TestRequest =>
    http.expectOne((r) => r.url === HISTORY_URL && r.params.get('page') === String(pageNumber));

  const render = async (today: DailyMessage, history: PagedResult<DailyMessage>) => {
    fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
    http.expectOne(TODAY_URL).flush(today);
    expectHistory(1).flush(history);
    await fixture.whenStable();
  };

  const typeIntent = (intent: string) => {
    const textarea = element().querySelector('textarea')!;
    textarea.value = intent;
    textarea.dispatchEvent(new Event('input'));
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HomePage],
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

  it("shows today's message and the first page of history", async () => {
    await render(
      message('m-1', 'Respira.'),
      page([message('m-1', 'Respira.'), message('m-0', 'Ayer.')]),
    );

    expect(text('.today')).toContain('Respira.');
    expect(element().querySelectorAll('.history li').length).toBe(2);
  });

  it('generates a message with the typed intent and refreshes the history', async () => {
    await render(message('m-1', 'Respira.'), page([message('m-1', 'Respira.')]));

    typeIntent('humorístico');
    button('Generar').click();
    const generate = http.expectOne(`${BASE_URL}/api/messages/generate`);
    expect(generate.request.body).toEqual({ intent: 'humorístico' });
    generate.flush(message('m-2', 'Ríete un poco.'));
    expectHistory(1).flush(page([message('m-2', 'Ríete un poco.'), message('m-1', 'Respira.')]));
    await fixture.whenStable();

    expect(text('.today')).toContain('Ríete un poco.');
    expect(element().querySelectorAll('.history li').length).toBe(2);
  });

  it('sends a null intent when the textarea is blank so the API uses the default', async () => {
    await render(message('m-1', 'Respira.'), page([]));

    typeIntent('   ');
    button('Generar').click();

    const generate = http.expectOne(`${BASE_URL}/api/messages/generate`);
    expect(generate.request.body).toEqual({ intent: null });
    generate.flush(message('m-2', 'Otro.'));
    expectHistory(1).flush(page([]));
  });

  it('saves the typed intent as the default preference', async () => {
    await render(message('m-1', 'Respira.'), page([]));

    typeIntent('reflexivo');
    button('Guardar como predeterminada').click();

    const put = http.expectOne(`${BASE_URL}/api/preferences`);
    expect(put.request.method).toBe('PUT');
    expect(put.request.body).toEqual({ defaultIntent: 'reflexivo' });
    put.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('pages through the history', async () => {
    await render(message('m-1', 'Respira.'), page([message('m-1', 'Respira.')], 1, 11));

    button('Siguiente').click();
    expectHistory(2).flush(page([message('m-0', 'Antiguo.')], 2, 11));
    await fixture.whenStable();

    expect(text('.history li')).toContain('Antiguo.');
    expect(button('Siguiente').disabled).toBeTrue();
  });

  it('shows the API error when the LLM is unavailable', async () => {
    fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
    http
      .expectOne(TODAY_URL)
      .flush(
        { status: 503, detail: 'LM Studio no responde.' },
        { status: 503, statusText: 'Service Unavailable' },
      );
    expectHistory(1).flush(page([]));
    await fixture.whenStable();

    expect(text('[role="alert"]')).toContain('LM Studio no responde.');
    expect(text('.today')).not.toContain('Cargando');
  });

  it('logs out by dropping the token and going to /login', async () => {
    TestBed.inject(TokenStorage).save('jwt');
    await render(message('m-1', 'Respira.'), page([]));

    button('Cerrar sesión').click();

    expect(TestBed.inject(TokenStorage).read()).toBeNull();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
