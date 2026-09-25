import { HttpErrorResponse } from '@angular/common/http';

import { UNREACHABLE_API_MESSAGE, UNKNOWN_ERROR_MESSAGE, problemMessage } from './problem-message';

const httpError = (status: number, error: unknown) =>
  new HttpErrorResponse({ status, error, url: 'http://api.test/api/x' });

describe('problemMessage', () => {
  it('explains that the API is unreachable on status 0', () => {
    expect(problemMessage(httpError(0, new ProgressEvent('error')))).toBe(UNREACHABLE_API_MESSAGE);
  });

  it('joins the validation errors of a 400 ProblemDetails', () => {
    const problem = {
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: { Email: ['Email inválido.'], Password: ['Muy corta.', 'Sin números.'] },
    };

    expect(problemMessage(httpError(400, problem))).toBe('Email inválido. Muy corta. Sin números.');
  });

  it('prefers detail over title', () => {
    const problem = { title: 'Unauthorized', status: 401, detail: 'Credenciales inválidas.' };

    expect(problemMessage(httpError(401, problem))).toBe('Credenciales inválidas.');
  });

  it('falls back to the title when there is no detail', () => {
    expect(problemMessage(httpError(503, { title: 'LLM no disponible', status: 503 }))).toBe(
      'LLM no disponible',
    );
  });

  it('uses a generic message when the body is not a ProblemDetails', () => {
    expect(problemMessage(httpError(500, 'boom'))).toBe(UNKNOWN_ERROR_MESSAGE);
    expect(problemMessage(new Error('boom'))).toBe(UNKNOWN_ERROR_MESSAGE);
  });
});
