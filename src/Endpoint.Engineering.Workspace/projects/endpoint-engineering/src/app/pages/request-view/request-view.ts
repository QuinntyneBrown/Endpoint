import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Card, Badge, StatusIndicator, Breadcrumb } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-view',
  imports: [CommonModule, Card, Badge, StatusIndicator, Breadcrumb, MatButtonModule, MatIconModule],
  templateUrl: './request-view.html',
  styleUrl: './request-view.scss'
})
export class RequestView implements OnInit {
  request?: ALaCarteRequest;
  loading = true;

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
        this.loading = false;
      });
    }
  }

  onEdit(): void {
    if (this.request) {
      this.router.navigate(['/requests', this.request.id, 'edit']);
    }
  }

  onExecute(): void {
    if (this.request) {
      this.router.navigate(['/requests', this.request.id, 'execute']);
    }
  }

  onDelete(): void {
    if (this.request && confirm(`Delete "${this.request.name}"?`)) {
      this.requestService.deleteRequest(this.request.id).subscribe(() => {
        this.router.navigate(['/requests']);
      });
    }
  }

  onBack(): void {
    this.router.navigate(['/requests']);
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      draft: 'Draft',
      ready: 'Ready',
      executing: 'Executing',
      completed: 'Completed',
      failed: 'Failed'
    };
    return labels[status] || status;
  }

  getStatusType(status: string): 'success' | 'warning' | 'error' | 'info' {
    const types: Record<string, 'success' | 'warning' | 'error' | 'info'> = {
      draft: 'info',
      ready: 'success',
      executing: 'warning',
      completed: 'success',
      failed: 'error'
    };
    return types[status] || 'info';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
