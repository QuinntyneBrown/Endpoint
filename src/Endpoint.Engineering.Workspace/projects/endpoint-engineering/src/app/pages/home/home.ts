import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { EmptyState, Button, QuickOption } from 'endpoint-engineering-components';

@Component({
  selector: 'app-home',
  imports: [EmptyState, Button, QuickOption],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomePage {
  constructor(private router: Router) {}

  onNewComposition(): void {
    this.router.navigate(['/request/create']);
  }

  onLoadConfiguration(): void {
    this.router.navigate(['/requests']);
  }

  onQuickOption(type: string): void {
    // Navigate to create page with context
    this.router.navigate(['/request/create'], { queryParams: { source: type } });
  }
}
