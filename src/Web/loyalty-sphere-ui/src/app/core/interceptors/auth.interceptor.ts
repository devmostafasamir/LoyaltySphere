import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * HTTP Interceptor that adds JWT authentication token to API requests.
 * 
 * In production, this would:
 * - Get token from AuthService
 * - Refresh expired tokens
 * - Handle 401 responses
 * 
 * For demo purposes, uses mock token from environment.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Only add auth header to API requests
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  // Get token from environment (in production, get from auth service)
  const token = environment.mockAuthToken;

  // Clone request and add authorization header
  const clonedRequest = req.clone({
    setHeaders: {
      'Authorization': `Bearer ${token}`
    }
  });

  return next(clonedRequest);
};
