import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { ToastMessage } from '../../../features/workspace/workspace.models';

@Component({
  selector: 'app-toast-stack',
  imports: [CommonModule],
  templateUrl: './toast-stack.component.html',
  styleUrl: './toast-stack.component.less',
})
export class ToastStackComponent {
  @Input() toasts: ToastMessage[] = [];
}
