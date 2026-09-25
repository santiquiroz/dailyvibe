import { HttpErrorResponse } from '@angular/common/http';

export const UNREACHABLE_API_MESSAGE = 'No se pudo contactar la API. ¿Está corriendo?';
export const UNKNOWN_ERROR_MESSAGE = 'Algo salió mal. Inténtalo de nuevo.';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function problemMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return UNKNOWN_ERROR_MESSAGE;
  }
  if (error.status === 0) {
    return UNREACHABLE_API_MESSAGE;
  }
  return describeProblem(error.error) ?? UNKNOWN_ERROR_MESSAGE;
}

function describeProblem(body: unknown): string | undefined {
  if (!isProblemDetails(body)) {
    return undefined;
  }
  return validationMessages(body.errors) ?? body.detail ?? body.title;
}

function validationMessages(errors: ProblemDetails['errors']): string | undefined {
  const messages = Object.values(errors ?? {}).flat();
  return messages.length > 0 ? messages.join(' ') : undefined;
}

function isProblemDetails(body: unknown): body is ProblemDetails {
  return typeof body === 'object' && body !== null;
}
