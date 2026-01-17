import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ep-help-text',
  imports: [CommonModule],
  templateUrl: './help-text.html',
  styleUrl: './help-text.scss',
})
export class HelpText {
  @Input() variant: 'info' | 'warning' | 'tip' = 'info';
  @Input() title: string = '';
  @Input() icon: string = '';

  get defaultIcon(): string {
    if (this.icon) return this.icon;
    const icons = {
      info: 'info',
      warning: 'warning',
      tip: 'lightbulb',
    };
    return icons[this.variant];
  }

  get helpTextClasses(): string {
    return `ep-help-text ep-help-text--${this.variant}`;
  }
}
