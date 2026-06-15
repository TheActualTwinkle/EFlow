import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-step-progress',
  standalone: true,
  templateUrl: './app-step-progress.component.html',
  styleUrl: './app-step-progress.component.less',
})
export class AppStepProgressComponent {
  @Input({ required: true }) steps: string[] = [];
  @Input({ required: true }) currentStep = 1;

  isCompleted(index: number): boolean {
    return index + 1 < this.currentStep;
  }

  isActive(index: number): boolean {
    return index + 1 === this.currentStep;
  }
}
