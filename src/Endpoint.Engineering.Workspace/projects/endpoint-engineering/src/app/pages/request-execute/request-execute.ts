import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Card, StatusIndicator } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest, ExecutionResult } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-execute',
  imports: [CommonModule, Card, StatusIndicator, MatButtonModule, MatIconModule],
  templateUrl: './request-execute.html',
  styleUrl: './request-execute.scss'
})
export class RequestExecute implements OnInit {
  request?: ALaCarteRequest;
  executionResult?: ExecutionResult;
  executing = false;
  executionProgress = 0;
  currentStep = '';
  logs: string[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private requestService: ALaCarteRequestService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.requestService.getRequest(id).subscribe(request => {
        this.request = request;
        this.startExecution();
      });
    }
  }

  startExecution(): void {
    if (!this.request) return;

    this.executing = true;
    this.logs = [];
    this.simulateProgress();

    this.requestService.executeRequest(this.request.id).subscribe({
      next: (result) => {
        this.executionResult = result;
        this.executing = false;
        this.executionProgress = 100;
        this.currentStep = 'Completed';
        this.logs.push('✓ Execution completed successfully!');
        
        if (result.outputPath) {
          this.logs.push(`✓ Output saved to: ${result.outputPath}`);
        }
        
        if (result.generatedFiles) {
          this.logs.push(`✓ Generated ${result.generatedFiles.length} files`);
        }
      },
      error: (err) => {
        this.executing = false;
        this.executionResult = {
          requestId: this.request!.id,
          status: 'error',
          errors: [err.message || 'Execution failed']
        };
        this.logs.push('✗ Execution failed');
      }
    });
  }

  simulateProgress(): void {
    const steps = [
      { progress: 10, step: 'Validating configuration', delay: 200 },
      { progress: 20, step: 'Cloning repositories', delay: 500 },
      { progress: 40, step: 'Processing templates', delay: 800 },
      { progress: 60, step: 'Generating solution structure', delay: 600 },
      { progress: 80, step: 'Writing files', delay: 400 },
      { progress: 90, step: 'Finalizing output', delay: 300 }
    ];

    let currentIndex = 0;
    const updateProgress = () => {
      if (currentIndex < steps.length && this.executing) {
        const { progress, step, delay } = steps[currentIndex];
        this.executionProgress = progress;
        this.currentStep = step;
        this.logs.push(`⋯ ${step}...`);
        currentIndex++;
        setTimeout(updateProgress, delay);
      }
    };

    updateProgress();
  }

  onViewOutput(): void {
    if (this.request) {
      this.router.navigate(['/output', this.request.id]);
    }
  }

  onBackToRequest(): void {
    if (this.request) {
      this.router.navigate(['/requests', this.request.id]);
    }
  }

  onBackToList(): void {
    this.router.navigate(['/requests']);
  }
}
