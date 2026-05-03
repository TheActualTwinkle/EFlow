import { ApplicationConfig, LOCALE_ID, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import {
  Bell,
  BookOpen,
  CalendarDays,
  CheckCircle2,
  Clock3,
  LogOut,
  LucideAngularModule,
  MapPin,
  Moon,
  Plus,
  RefreshCw,
  ShieldCheck,
  Sun,
  Trash2,
  Users,
} from 'lucide-angular';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: LOCALE_ID, useValue: 'ru-RU' },
    provideRouter(routes),
    importProvidersFrom(
      LucideAngularModule.pick({
        Bell,
        BookOpen,
        CalendarDays,
        CheckCircle2,
        Clock3,
        LogOut,
        MapPin,
        Moon,
        Plus,
        RefreshCw,
        ShieldCheck,
        Sun,
        Trash2,
        Users,
      }),
    ),
    provideHttpClient(
      withInterceptors([
        (request, next) => {
          return next(
            request.clone({
              withCredentials: true,
            }),
          );
        },
      ]),
    ),
  ],
};
