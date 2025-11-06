import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const isAuth = authService.isAuthenticated();
  console.log('[Auth Guard] Checking authentication for:', state.url, 'Result:', isAuth);
  
  if (isAuth) {
    console.log('[Auth Guard] Access granted');
    return true;
  }
  
  console.log('[Auth Guard] Access denied, redirecting to login');
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
