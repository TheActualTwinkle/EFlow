import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  EventEmitter,
  forwardRef,
  HostListener,
  Input,
  OnDestroy,
  Output,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { AppIconComponent } from '../app-icon/app-icon.component';
import { AppSelectOption } from './app-select-option';

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule, AppIconComponent],
  templateUrl: './app-select.component.html',
  styleUrl: './app-select.component.less',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppSelectComponent),
      multi: true,
    },
  ],
})
export class AppSelectComponent implements ControlValueAccessor, OnDestroy {
  private static activeSelect: AppSelectComponent | null = null;

  @Input() options: AppSelectOption[] = [];
  @Input() placeholder = 'Выберите значение';
  @Input() emptyLabel = 'Нет вариантов';
  @Input() suffixIcon = '';
  @Input() ariaLabel = '';
  @Input() disabled = false;
  @Output() valueChange = new EventEmitter<string>();

  value = '';
  open = false;

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private readonly elementRef: ElementRef<HTMLElement>) {}

  get selectedLabel(): string {
    if (!this.options.length && this.disabled) {
      return this.emptyLabel;
    }

    return this.options.find((option) => option.value === this.value)?.label ?? this.placeholder;
  }

  toggle(): void {
    if (this.disabled || !this.options.length) {
      return;
    }

    if (this.open) {
      this.close();
    } else {
      AppSelectComponent.activeSelect?.close();
      this.open = true;
      AppSelectComponent.activeSelect = this;
    }
    this.onTouched();
  }

  select(option: AppSelectOption): void {
    if (option.disabled) {
      return;
    }

    this.value = option.value;
    this.close();
    this.onChange(option.value);
    this.valueChange.emit(option.value);
    this.onTouched();
  }

  writeValue(value: string | null | undefined): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled) {
      this.close();
    }
  }

  ngOnDestroy(): void {
    if (AppSelectComponent.activeSelect === this) {
      AppSelectComponent.activeSelect = null;
    }
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }

  private close(): void {
    this.open = false;
    if (AppSelectComponent.activeSelect === this) {
      AppSelectComponent.activeSelect = null;
    }
  }
}
