import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'ep-card',
  imports: [CommonModule, MatCardModule],
  templateUrl: './card.html',
  styleUrl: './card.scss',
})
export class Card {
  @Input() variant: 'default' | 'elevated' | 'outlined' = 'default';
  @Input() padding: 'none' | 'small' | 'medium' | 'large' = 'medium';
  @Input() hoverable: boolean = false;

  get cardClasses(): string {
    const classes = ['ep-card'];
    classes.push(`ep-card--padding-${this.padding}`);
    if (this.hoverable) {
      classes.push('ep-card--hoverable');
    }
    return classes.join(' ');
  }

  get matAppearance(): 'outlined' | 'raised' {
    return this.variant === 'outlined' ? 'outlined' : 'raised';
  }
}
