import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnDestroy, Output, SimpleChanges } from '@angular/core';

import { ToastMessage } from '../../../features/workspace/workspace.models';

@Component({
  selector: 'app-toast-stack',
  imports: [CommonModule],
  templateUrl: './toast-stack.component.html',
  styleUrl: './toast-stack.component.less',
})
export class ToastStackComponent implements OnChanges, OnDestroy {
  @Input() toasts: ToastMessage[] = [];
  @Output() closeToast = new EventEmitter<number>();
  @Output() runAction = new EventEmitter<number>();

  readonly dismissingIds = new Set<number>();
  private readonly timers = new Map<number, number>();

  ngOnDestroy(): void {
    this.timers.forEach((timer) => window.clearTimeout(timer));
    this.timers.clear();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['toasts']) {
      return;
    }

    const activeIds = new Set(this.toasts.map((toast) => toast.id));

    this.timers.forEach((timer, id) => {
      if (!activeIds.has(id)) {
        window.clearTimeout(timer);
        this.timers.delete(id);
      }
    });

    for (const toast of this.toasts) {
      if (this.timers.has(toast.id)) {
        continue;
      }

      const timer = window.setTimeout(() => this.close(toast.id), toast.durationMs);
      this.timers.set(toast.id, timer);
    }
  }

  progressStyle(toast: ToastMessage): Record<string, string> {
    return { '--toast-duration': `${toast.durationMs}ms` };
  }

  close(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      window.clearTimeout(timer);
      this.timers.delete(id);
    }

    this.dismissingIds.add(id);
    window.setTimeout(() => {
      this.dismissingIds.delete(id);
      this.closeToast.emit(id);
    }, 220);
  }
}
