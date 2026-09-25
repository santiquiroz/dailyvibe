import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { TokenStorage } from './token-storage';

export const authGuard: CanActivateFn = () =>
  inject(TokenStorage).read() ? true : inject(Router).createUrlTree(['/login']);
