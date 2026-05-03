import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-error-modal',
  imports: [CommonModule],
  templateUrl: './error-modal.component.html',
  styleUrl: './error-modal.component.less',
})
export class ErrorModalComponent {
  @Input() error: string | null = null;
  @Output() close = new EventEmitter<void>();
}
