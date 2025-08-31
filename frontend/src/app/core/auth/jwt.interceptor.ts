import { HttpEvent, HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastService } from '../api/toast.service';
import { catchError, map, throwError } from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  const toasts = inject(ToastService);
  return next(req).pipe(
    map((event: HttpEvent<any>) => {
      if (event instanceof HttpResponse) {
        const body = event.body as any;
        if (body && typeof body === 'object' && 'statusCode' in body && 'message' in body && 'data' in body) {
          return event.clone({ body: body.data });
        }
      }
      return event;
    }),
    catchError((err: any) => {
      const msg = err?.error?.message || err?.statusText || 'Request failed';
      toasts.error(msg);
      return throwError(() => err);
    })
  );
};


