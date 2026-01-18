import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BadgeVariant = 'success' | 'processing' | 'pending' | 'error';

@Component({
  selector: 'ee-badge',
  imports: [CommonModule],
  templateUrl: './badge.html',
  styleUrl: './badge.scss',
})
export class Badge {
  label = input.required<string>();
  variant = input<BadgeVariant>('pending');
}
