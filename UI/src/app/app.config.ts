import { ApplicationConfig, LOCALE_ID, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { finalize } from 'rxjs';

import { routes } from './app.routes';
import { HttpActivityService } from './core/http-activity.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: LOCALE_ID, useValue: 'ru-RU' },
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        (request, next) => {
          const activity = inject(HttpActivityService);
          activity.start();

          return next(
            request.clone({
              withCredentials: true,
            }),
          ).pipe(finalize(() => activity.stop()));
        },
      ]),
    ),
  ],
};
