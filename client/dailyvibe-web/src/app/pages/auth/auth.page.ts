import { Component, computed, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';

import { AuthResult, Credentials } from '../../core/api/api.models';
import { ApiService } from '../../core/api/api.service';
import { TokenStorage } from '../../core/auth/token-storage';
import { problemMessage } from '../../core/errors/problem-message';

export type AuthMode = 'login' | 'register';

const PASSWORD_MIN_LENGTH = 8;
const EMAIL_MAX_LENGTH = 256;

interface AuthCopy {
  title: string;
  submitLabel: string;
  switchPrompt: string;
  switchLabel: string;
  switchLink: string;
}

const COPY: Record<AuthMode, AuthCopy> = {
  login: {
    title: 'Inicia sesión',
    submitLabel: 'Entrar',
    switchPrompt: '¿No tienes cuenta?',
    switchLabel: 'Regístrate',
    switchLink: '/register',
  },
  register: {
    title: 'Crea tu cuenta',
    submitLabel: 'Registrarme',
    switchPrompt: '¿Ya tienes cuenta?',
    switchLabel: 'Inicia sesión',
    switchLink: '/login',
  },
};

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './auth.page.html',
  styleUrl: './auth.page.css',
})
export class AuthPage {
  readonly mode = input.required<AuthMode>();

  private readonly api = inject(ApiService);
  private readonly tokens = inject(TokenStorage);
  private readonly router = inject(Router);

  protected readonly copy = computed(() => COPY[this.mode()]);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly passwordMinLength = PASSWORD_MIN_LENGTH;

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(EMAIL_MAX_LENGTH)]],
    password: ['', [Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.authenticate(this.form.getRawValue()).subscribe({
      next: (result) => this.startSession(result),
      error: (error: unknown) => this.fail(error),
    });
  }

  private authenticate(credentials: Credentials): Observable<AuthResult> {
    return this.mode() === 'register'
      ? this.api.register(credentials)
      : this.api.login(credentials);
  }

  private startSession(result: AuthResult): void {
    this.tokens.save(result.token);
    void this.router.navigateByUrl('/');
  }

  private fail(error: unknown): void {
    this.error.set(problemMessage(error));
    this.pending.set(false);
  }
}
