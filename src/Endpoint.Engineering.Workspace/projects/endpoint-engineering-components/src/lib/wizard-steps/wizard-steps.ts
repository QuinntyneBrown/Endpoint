import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface WizardStep {
  label: string;
  completed?: boolean;
}

@Component({
  selector: 'ee-wizard-steps',
  imports: [CommonModule],
  templateUrl: './wizard-steps.html',
  styleUrl: './wizard-steps.scss',
})
export class WizardSteps {
  steps = input.required<WizardStep[]>();
  currentStep = input<number>(0);
}
