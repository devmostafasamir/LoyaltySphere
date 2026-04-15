import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * HTTP Interceptor that adds tenant identification to all API requests.
 * 
 * Adds X-Tenant-Id header to every outgoing request.
 * In production, tenant would come from:
 * - Subdomain (tenant.loyaltysphere.com)
 * - JWT token claims
 * - User session
 * 
 * For demo purposes, uses environment configuration.
 */
export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  // Only add tenant header to API requests
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  // Get tenant ID from environment (in production, get from auth service)
  const tenantId = environment.tenantId;

  // Clone request and add tenant header
  const clonedRequest = req.clone({
    setHeaders: {
      'X-Tenant-Id': tenantId
    }
  });

  return next(clonedRequest);
};
