import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { PageHeader, Button, FormSection } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { OutputType } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-create',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeader,
    Button,
    FormSection
  ],
  templateUrl: './request-create.html',
  styleUrl: './request-create.scss'
})
export class RequestCreatePage {
  protected readonly name = signal('');
  protected readonly solutionName = signal('');
  protected readonly outputDirectory = signal('');
  protected readonly outputType = signal<OutputType>(OutputType.DotNetSolution);
  
  protected readonly outputTypes = Object.values(OutputType);
  protected readonly OutputType = OutputType;

  constructor(
    private requestService: ALaCarteRequestService,
    private router: Router
  ) {}

  onCancel(): void {
    this.router.navigate(['/requests']);
  }

  onSave(): void {
    const request = {
      name: this.name(),
      solutionName: this.solutionName(),
      outputDirectory: this.outputDirectory(),
      outputType: this.outputType(),
      repositories: []
    };

    this.requestService.create(request);
    this.router.navigate(['/requests']);
  }

  isValid(): boolean {
    return !!(this.name() && this.solutionName() && this.outputDirectory());
  }
}
