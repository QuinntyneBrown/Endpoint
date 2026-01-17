import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ep-button',
  imports: [CommonModule],
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class Button {
  @Input() variant: 'primary' | 'secondary' | 'danger' | 'icon' = 'primary';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() disabled: boolean = false;
  @Input() fullWidth: boolean = false;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Output() click = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (!this.disabled) {
      this.click.emit(event);
    }
  }

  get buttonClasses(): string {
    const classes = ['ep-button'];
    classes.push(`ep-button--${this.variant}`);
    classes.push(`ep-button--${this.size}`);
    if (this.fullWidth) {
      classes.push('ep-button--full-width');
    }
    if (this.disabled) {
      classes.push('ep-button--disabled');
    }
    return classes.join(' ');
  }
}
