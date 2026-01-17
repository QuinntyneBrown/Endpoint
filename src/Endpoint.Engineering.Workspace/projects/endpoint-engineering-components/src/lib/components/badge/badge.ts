import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ep-badge',
  imports: [CommonModule],
  templateUrl: './badge.html',
  styleUrl: './badge.scss',
})
export class Badge {
  @Input() variant: 'primary' | 'success' | 'warning' | 'danger' | 'neutral' = 'neutral';
  @Input() size: 'small' | 'medium' = 'medium';
  @Input() dot: boolean = false;

  get badgeClasses(): string {
    const classes = ['ep-badge'];
    classes.push(`ep-badge--${this.variant}`);
    classes.push(`ep-badge--${this.size}`);
    if (this.dot) {
      classes.push('ep-badge--dot');
    }
    return classes.join(' ');
  }
}
