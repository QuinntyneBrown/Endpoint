import { Component } from '@angular/core';
import { EmptyState, Button, Card } from 'endpoint-engineering-components';

@Component({
  selector: 'app-home',
  imports: [EmptyState, Button, Card],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  onNewComposition(): void {
    console.log('Create new composition');
  }

  onLoadConfiguration(): void {
    console.log('Load saved configuration');
  }

  onQuickStart(option: string): void {
    console.log('Quick start:', option);
  }
}
