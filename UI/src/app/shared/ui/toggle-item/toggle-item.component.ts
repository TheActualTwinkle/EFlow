import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { AppIconComponent } from '../app-icon/app-icon.component';
import { HoverHintDirective } from '../hover-hint/hover-hint.directive';

@Component({
  selector: 'app-toggle-item',
  imports: [CommonModule, AppIconComponent, HoverHintDirective],
  templateUrl: './toggle-item.component.html',
  styleUrl: './toggle-item.component.less',
})
export class ToggleItemComponent {
  @Input({ required: true }) title = '';
  @Input() subtitle = '';
  @Input() warningHint = '';
  @Input() selected = false;
  @Output() toggle = new EventEmitter<void>();
}
