import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Card, FormInput, WizardSteps } from 'endpoint-engineering-components';
import { CompositionInput } from '../../models/alacarte-request.model';

interface WizardStep {
  label: string;
  completed: boolean;
}

@Component({
  selector: 'app-composition-wizard',
  imports: [CommonModule, FormsModule, Card, FormInput, WizardSteps, MatButtonModule, MatIconModule],
  templateUrl: './composition-wizard.html',
  styleUrl: './composition-wizard.scss'
})
export class CompositionWizard {
  currentStep = 0;
  steps: WizardStep[] = [
    { label: 'Basic Info', completed: false },
    { label: 'Select Components', completed: false },
    { label: 'Configure', completed: false }
  ];

  get wizardSteps() {
    return this.steps.map((s, i) => ({
      label: s.label,
      active: i === this.currentStep,
      completed: s.completed
    }));
  }

  composition: CompositionInput = {
    solutionName: '',
    outputDirectory: '',
    description: '',
    targetFramework: '.NET 8',
    includeTests: true,
    generateDocs: true,
    addDocker: false,
    addCICD: true
  };

  frameworks = ['.NET 8 (Recommended)', '.NET 7', '.NET 6 (LTS)'];

  constructor(private router: Router) {}

  onNext(): void {
    if (this.currentStep < this.steps.length - 1) {
      this.steps[this.currentStep].completed = true;
      this.currentStep++;
    } else {
      this.onComplete();
    }
  }

  onPrevious(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }

  onComplete(): void {
    console.log('Composition complete', this.composition);
    this.router.navigate(['/requests/create']);
  }

  canProceed(): boolean {
    if (this.currentStep === 0) {
      return !!(this.composition.solutionName && this.composition.outputDirectory);
    }
    return true;
  }
}
