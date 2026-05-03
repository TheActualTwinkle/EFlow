import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class HttpActivityService {
  private readonly activeRequests = signal(0);

  readonly busy = computed(() => this.activeRequests() > 0);

  start(): void {
    this.activeRequests.update((count) => count + 1);
  }

  stop(): void {
    this.activeRequests.update((count) => Math.max(0, count - 1));
  }
}
