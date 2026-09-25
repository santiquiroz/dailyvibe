import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { DailyMessage, PagedResult } from '../../core/api/api.models';
import { ApiService } from '../../core/api/api.service';
import { TokenStorage } from '../../core/auth/token-storage';
import { problemMessage } from '../../core/errors/problem-message';

const INTENT_MAX_LENGTH = 500;
const HISTORY_PAGE_SIZE = 10;

export function normalizeIntent(value: string): string | null {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

@Component({
  selector: 'app-home-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './home.page.html',
  styleUrl: './home.page.css',
})
export class HomePage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly tokens = inject(TokenStorage);
  private readonly router = inject(Router);

  protected readonly intentMaxLength = INTENT_MAX_LENGTH;
  protected readonly intent = new FormControl('', {
    nonNullable: true,
    validators: [Validators.maxLength(INTENT_MAX_LENGTH)],
  });

  protected readonly today = signal<DailyMessage | null>(null);
  protected readonly loadingToday = signal(true);
  protected readonly history = signal<PagedResult<DailyMessage> | null>(null);
  protected readonly generating = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  private readonly intentValue = toSignal(this.intent.valueChanges, {
    initialValue: this.intent.value,
  });
  protected readonly canSaveAsDefault = computed(
    () => !this.saving() && normalizeIntent(this.intentValue()) !== null,
  );

  protected readonly hasPrevious = computed(() => (this.history()?.page ?? 1) > 1);
  protected readonly hasNext = computed(() => {
    const history = this.history();
    return history !== null && history.page * history.size < history.totalCount;
  });

  ngOnInit(): void {
    this.loadToday();
    this.loadHistory(1);
  }

  protected generate(): void {
    if (this.intent.invalid) {
      return;
    }
    this.generating.set(true);
    this.clearFeedback();
    this.api.generate(normalizeIntent(this.intent.value)).subscribe({
      next: (message) => this.showGenerated(message),
      error: (error: unknown) => this.fail(error),
    });
  }

  protected saveAsDefault(): void {
    const intent = normalizeIntent(this.intent.value);
    if (intent === null || this.intent.invalid || this.saving()) {
      return;
    }
    this.saving.set(true);
    this.clearFeedback();
    this.api
      .updatePreferences(intent)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => this.notice.set('Intención guardada como predeterminada.'),
        error: (error: unknown) => this.fail(error),
      });
  }

  protected previousPage(): void {
    this.loadHistory((this.history()?.page ?? 1) - 1);
  }

  protected nextPage(): void {
    this.loadHistory((this.history()?.page ?? 1) + 1);
  }

  protected logout(): void {
    this.tokens.clear();
    void this.router.navigateByUrl('/login');
  }

  private loadToday(): void {
    this.api
      .getToday()
      .pipe(finalize(() => this.loadingToday.set(false)))
      .subscribe({
        next: (message) => this.today.set(message),
        error: (error: unknown) => this.fail(error),
      });
  }

  private loadHistory(page: number): void {
    this.api.getHistory(page, HISTORY_PAGE_SIZE).subscribe({
      next: (history) => this.history.set(history),
      error: (error: unknown) => this.fail(error),
    });
  }

  private showGenerated(message: DailyMessage): void {
    this.today.set(message);
    this.generating.set(false);
    this.loadHistory(1);
  }

  private clearFeedback(): void {
    this.error.set(null);
    this.notice.set(null);
  }

  private fail(error: unknown): void {
    this.error.set(problemMessage(error));
    this.generating.set(false);
  }
}
