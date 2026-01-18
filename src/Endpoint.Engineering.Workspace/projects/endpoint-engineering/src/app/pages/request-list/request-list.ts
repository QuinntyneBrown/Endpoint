import { Component, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { StatsBar, SearchBox, Button, IconButton, Tag } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-list',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    StatsBar,
    SearchBox,
    Button,
    IconButton,
    Tag
  ],
  templateUrl: './request-list.html',
  styleUrl: './request-list.scss'
})
export class RequestListPage {
  private readonly searchTerm = signal('');
  protected readonly requests = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const allRequests = this.requestService.getAll();
    if (!term) return allRequests;
    return allRequests.filter(r =>
      r.name.toLowerCase().includes(term) ||
      r.solutionName.toLowerCase().includes(term)
    );
  });

  protected readonly displayedColumns = ['name', 'solutionName', 'repositories', 'created', 'lastUsed', 'actions'];

  protected readonly stats = computed(() => {
    const reqs = this.requestService.getAll();
    const totalExecutions = reqs.reduce((sum, r) => sum + r.executionCount, 0);
    const recentlyUsed = reqs.filter(r => {
      if (!r.lastUsed) return false;
      const daysSinceUsed = (Date.now() - r.lastUsed.getTime()) / (1000 * 60 * 60 * 24);
      return daysSinceUsed <= 7;
    }).length;

    return [
      { label: 'Total Requests', value: reqs.length.toString() },
      { label: 'Executions', value: totalExecutions.toString() },
      { label: 'Used This Week', value: recentlyUsed.toString() }
    ];
  });

  constructor(
    private requestService: ALaCarteRequestService,
    private router: Router
  ) {}

  onSearch(term: string): void {
    this.searchTerm.set(term);
  }

  onNewRequest(): void {
    this.router.navigate(['/request/create']);
  }

  onView(request: ALaCarteRequest): void {
    this.router.navigate(['/request', request.id]);
  }

  onEdit(request: ALaCarteRequest): void {
    this.router.navigate(['/request', request.id, 'edit']);
  }

  onExecute(request: ALaCarteRequest): void {
    this.router.navigate(['/request', request.id, 'execute']);
  }

  onDelete(request: ALaCarteRequest): void {
    if (confirm(`Are you sure you want to delete "${request.name}"?`)) {
      this.requestService.delete(request.id);
    }
  }

  formatDate(date: Date | undefined): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleDateString();
  }
}
