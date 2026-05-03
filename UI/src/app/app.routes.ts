import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', children: [] },
  { path: 'overview', children: [] },
  { path: 'slots', children: [] },
  { path: 'bookings', children: [] },
  { path: 'admin', children: [] },
  { path: 'admin/:section', children: [] },
  { path: '**', redirectTo: 'overview' },
];
