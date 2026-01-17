import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { EmptyState, Button, Card } from 'endpoint-engineering-components';

@Component({
  selector: 'app-home',
  imports: [EmptyState, Button, Card],
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
