import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { LoadingService } from '@services';
import { finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  console.log('[LOADING] Request started:', req.url);
  loadingService.show();

  return next(req).pipe(
    finalize(() => {
      console.log('[LOADING] Request finished:', req.url);
      loadingService.hide();
    })
  );
};
