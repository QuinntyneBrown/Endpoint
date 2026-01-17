import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ep-button',
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class Button {
  @Input() variant: 'primary' | 'secondary' | 'danger' | 'icon' = 'primary';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() disabled: boolean = false;
  @Input() fullWidth: boolean = false;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Output() buttonClick = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (!this.disabled) {
      this.buttonClick.emit(event);
    }
  }

  get hostClasses(): string {
    const classes = ['ep-button'];
    classes.push(`ep-button--${this.size}`);
    if (this.fullWidth) {
      classes.push('ep-button--full-width');
    }
    return classes.join(' ');
  }

  get matColor(): 'primary' | 'accent' | 'warn' | undefined {
    switch (this.variant) {
      case 'primary':
        return 'primary';
      case 'danger':
        return 'warn';
      default:
        return undefined;
    }
  }
}
