import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmptyState, Card } from 'endpoint-engineering-components';

@Component({
  selector: 'app-home',
  imports: [EmptyState, Card, MatButtonModule, MatIconModule],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  constructor(private router: Router) {}

  onNewComposition(): void {
    this.router.navigate(['/compose']);
  }

  onLoadConfiguration(): void {
    this.router.navigate(['/configurations']);
  }

  onQuickStart(option: string): void {
    console.log('Quick start:', option);
    this.router.navigate(['/requests']);
  }
}
