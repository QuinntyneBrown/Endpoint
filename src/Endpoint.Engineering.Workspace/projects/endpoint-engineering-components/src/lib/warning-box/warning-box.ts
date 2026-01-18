import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type WarningVariant = 'warning' | 'error' | 'info' | 'success';

@Component({
  selector: 'ee-warning-box',
  imports: [CommonModule],
  templateUrl: './warning-box.html',
  styleUrl: './warning-box.scss',
})
export class WarningBox {
  message = input.required<string>();
  variant = input<WarningVariant>('warning');
  title = input<string>('');

  getIcon(): string {
    switch (this.variant()) {
      case 'error':
        return 'error_outline';
      case 'info':
        return 'info_outline';
      case 'success':
        return 'check_circle_outline';
      default:
        return 'warning_amber';
    }
  }
}
