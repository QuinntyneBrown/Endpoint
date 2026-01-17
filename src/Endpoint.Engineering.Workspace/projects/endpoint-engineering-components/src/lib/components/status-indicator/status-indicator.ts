import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ep-status-indicator',
  imports: [CommonModule, MatIconModule],
  templateUrl: './status-indicator.html',
  styleUrl: './status-indicator.scss',
})
export class StatusIndicator {
  @Input() status: 'success' | 'error' | 'warning' | 'info' | 'loading' = 'info';
  @Input() text: string = '';
  @Input() showIcon: boolean = true;

  get iconName(): string {
    const icons = {
      success: 'check_circle',
      error: 'error',
      warning: 'warning',
      info: 'info',
      loading: 'autorenew',
    };
    return icons[this.status];
  }

  get statusClasses(): string {
    return `ep-status-indicator ep-status-indicator--${this.status}`;
  }
}
