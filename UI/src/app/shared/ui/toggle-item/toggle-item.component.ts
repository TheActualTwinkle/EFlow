import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-toggle-item',
  imports: [CommonModule],
  templateUrl: './toggle-item.component.html',
  styleUrl: './toggle-item.component.less',
})
export class ToggleItemComponent {
  @Input({ required: true }) title = '';
  @Input() subtitle = '';
  @Input() selected = false;
  @Output() toggle = new EventEmitter<void>();
}
