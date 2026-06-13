import { DOCUMENT } from '@angular/common';
import { Directive, ElementRef, HostListener, inject, input, OnDestroy } from '@angular/core';

@Directive({
  selector: '[appHoverHint]',
  standalone: true,
})
export class HoverHintDirective implements OnDestroy {
  readonly appHoverHint = input('');

  private readonly gap = 22;
  private readonly sideGap = 20;
  private readonly viewportPadding = 10;
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

    this.positionTooltip(event);
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

  private positionTooltip(event: MouseEvent): void {
    if (!this.tooltip) {
      return;
    }

    const tooltip = this.tooltip;
    const rect = tooltip.getBoundingClientRect();
    const viewportWidth = this.document.documentElement.clientWidth;
    const viewportHeight = this.document.documentElement.clientHeight;
    const preferred = this.tooltipPositions(event, rect.width, rect.height);
    const position = preferred.find((item) => this.fitsViewport(item.left, item.top, rect.width, rect.height, viewportWidth, viewportHeight)) ?? preferred[0];

    tooltip.style.left = `${this.clamp(position.left, this.viewportPadding, viewportWidth - rect.width - this.viewportPadding)}px`;
    tooltip.style.top = `${this.clamp(position.top, this.viewportPadding, viewportHeight - rect.height - this.viewportPadding)}px`;
  }

  private tooltipPositions(event: MouseEvent, width: number, height: number): Array<{ left: number; top: number }> {
    return [
      {
        left: event.clientX - width / 2,
        top: event.clientY - height - this.gap,
      },
      {
        left: event.clientX - width / 2,
        top: event.clientY + this.gap,
      },
      {
        left: event.clientX + this.gap,
        top: event.clientY + this.sideGap,
      },
      {
        left: event.clientX - width - this.gap,
        top: event.clientY + this.sideGap,
      },
    ];
  }

  private fitsViewport(left: number, top: number, width: number, height: number, viewportWidth: number, viewportHeight: number): boolean {
    return (
      left >= this.viewportPadding &&
      top >= this.viewportPadding &&
      left + width <= viewportWidth - this.viewportPadding &&
      top + height <= viewportHeight - this.viewportPadding
    );
  }

  private clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), Math.max(min, max));
  }
}
