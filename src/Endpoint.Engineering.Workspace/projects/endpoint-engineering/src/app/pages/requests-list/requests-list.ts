import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Card, SearchBox, EmptyState, StatusIndicator } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-requests-list',
  imports: [CommonModule, Card, SearchBox, EmptyState, StatusIndicator, MatButtonModule, MatIconModule],
  templateUrl: './requests-list.html',
  styleUrl: './requests-list.scss'
})
export class RequestsList implements OnInit {
  requests: ALaCarteRequest[] = [];
  filteredRequests: ALaCarteRequest[] = [];
  searchQuery = '';

  constructor(
    private requestService: ALaCarteRequestService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.requestService.getRequests().subscribe(requests => {
      this.requests = requests;
      this.filteredRequests = requests;
    });
  }

  onSearch(query: string): void {
    this.searchQuery = query;
    if (!query.trim()) {
      this.filteredRequests = this.requests;
      return;
    }
    
    const lowerQuery = query.toLowerCase();
    this.filteredRequests = this.requests.filter(req =>
      req.name.toLowerCase().includes(lowerQuery) ||
      req.solutionName.toLowerCase().includes(lowerQuery) ||
      req.description?.toLowerCase().includes(lowerQuery)
    );
  }

  onCreateNew(): void {
    this.router.navigate(['/requests/create']);
  }

  onViewRequest(request: ALaCarteRequest): void {
    this.router.navigate(['/requests', request.id]);
  }

  onEditRequest(request: ALaCarteRequest, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/requests', request.id, 'edit']);
  }

  onExecuteRequest(request: ALaCarteRequest, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/requests', request.id, 'execute']);
  }

  onDeleteRequest(request: ALaCarteRequest, event: Event): void {
    event.stopPropagation();
    // Delete dialog will be shown
    if (confirm(`Delete "${request.name}"?`)) {
      this.requestService.deleteRequest(request.id).subscribe();
    }
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
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
