import { DOCUMENT } from '@angular/common';
import { Directive, ElementRef, HostListener, inject, input, OnDestroy } from '@angular/core';

@Directive({
  selector: '[appHoverHint]',
  standalone: true,
})
export class HoverHintDirective implements OnDestroy {
  readonly appHoverHint = input('');

  private readonly document = inject(DOCUMENT);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private tooltip: HTMLDivElement | null = null;

  @HostListener('mouseenter', ['$event'])
  show(event: MouseEvent): void {
    const text = this.appHoverHint().trim();

    if (!text) {
      this.hide();
      return;
    }

    this.ensureTooltip().textContent = text;
    this.host.nativeElement.removeAttribute('title');
    this.move(event);
  }

  @HostListener('mousemove', ['$event'])
  move(event: MouseEvent): void {
    if (!this.tooltip) {
      return;
    }

    this.tooltip.style.left = `${event.clientX}px`;
    this.tooltip.style.top = `${event.clientY}px`;
  }

  @HostListener('mouseleave')
  hide(): void {
    this.tooltip?.remove();
    this.tooltip = null;
  }

  ngOnDestroy(): void {
    this.hide();
  }

  private ensureTooltip(): HTMLDivElement {
    if (this.tooltip) {
      return this.tooltip;
    }

    const tooltip = this.document.createElement('div');
    tooltip.className = 'app-hover-hint-tooltip';
    this.document.body.appendChild(tooltip);
    this.tooltip = tooltip;

    return tooltip;
  }
}
