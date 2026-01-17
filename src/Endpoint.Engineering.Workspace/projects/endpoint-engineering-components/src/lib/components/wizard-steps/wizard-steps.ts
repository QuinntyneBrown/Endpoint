import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

export interface WizardStep {
  label: string;
  completed?: boolean;
  active?: boolean;
}

@Component({
  selector: 'ep-wizard-steps',
  imports: [CommonModule, MatIconModule],
  templateUrl: './wizard-steps.html',
  styleUrl: './wizard-steps.scss',
})
export class WizardSteps {
  @Input() steps: WizardStep[] = [];
  @Input() currentStep: number = 0;

  isActive(index: number): boolean {
    return index === this.currentStep;
  }

  isCompleted(index: number): boolean {
    return index < this.currentStep || (this.steps[index]?.completed ?? false);
  }
}
