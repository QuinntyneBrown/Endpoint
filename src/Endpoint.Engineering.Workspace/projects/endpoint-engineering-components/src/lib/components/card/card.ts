import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ep-card',
  imports: [CommonModule],
  templateUrl: './card.html',
  styleUrl: './card.scss',
})
export class Card {
  @Input() variant: 'default' | 'elevated' | 'outlined' = 'default';
  @Input() padding: 'none' | 'small' | 'medium' | 'large' = 'medium';
  @Input() hoverable: boolean = false;

  get cardClasses(): string {
    const classes = ['ep-card'];
    classes.push(`ep-card--${this.variant}`);
    classes.push(`ep-card--padding-${this.padding}`);
    if (this.hoverable) {
      classes.push('ep-card--hoverable');
    }
    return classes.join(' ');
  }
}
