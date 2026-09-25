import { TestBed } from '@angular/core/testing';

import { TOKEN_STORAGE_KEY, TokenStorage } from './token-storage';

describe('TokenStorage', () => {
  let storage: TokenStorage;

  beforeEach(() => {
    localStorage.clear();
    storage = TestBed.inject(TokenStorage);
  });

  afterEach(() => localStorage.clear());

  it('returns null when nothing is stored', () => {
    expect(storage.read()).toBeNull();
  });

  it('persists the token in localStorage', () => {
    storage.save('abc.def.ghi');

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBe('abc.def.ghi');
    expect(storage.read()).toBe('abc.def.ghi');
  });

  it('forgets the token on clear', () => {
    storage.save('abc.def.ghi');

    storage.clear();

    expect(storage.read()).toBeNull();
  });
});
