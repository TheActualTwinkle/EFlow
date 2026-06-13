import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-icon',
  standalone: true,
  templateUrl: './app-icon.component.html',
  styleUrl: './app-icon.component.less',
})
export class AppIconComponent {
  readonly name = input.required<string>();
  readonly href = computed(() => `/assets/icons/lucide-sprite.svg#${this.name()}`);
}
