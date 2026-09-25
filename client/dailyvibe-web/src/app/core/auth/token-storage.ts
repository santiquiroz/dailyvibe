import { Injectable } from '@angular/core';

export const TOKEN_STORAGE_KEY = 'dailyvibe.token';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  read(): string | null {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  save(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }
}
