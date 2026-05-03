import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const themeStorageKey = 'eflow.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly modeState = signal<ThemeMode>(this.readInitialTheme());

  readonly mode = this.modeState.asReadonly();

  constructor(@Inject(DOCUMENT) private readonly document: Document) {
    this.apply(this.modeState());
  }

  toggle(): void {
    const next = this.modeState() === 'dark' ? 'light' : 'dark';
    localStorage.setItem(themeStorageKey, next);
    this.modeState.set(next);
    this.apply(next);
  }

  private readInitialTheme(): ThemeMode {
    const stored = localStorage.getItem(themeStorageKey);

    if (stored === 'light' || stored === 'dark') {
      return stored;
    }

    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private apply(mode: ThemeMode): void {
    this.document.documentElement.dataset['theme'] = mode;
  }
}
