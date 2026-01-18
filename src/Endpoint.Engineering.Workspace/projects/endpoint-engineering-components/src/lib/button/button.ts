import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ButtonVariant = 'primary' | 'secondary' | 'danger';

@Component({
  selector: 'ee-button',
  imports: [CommonModule],
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class Button {
  variant = input<ButtonVariant>('primary');
  icon = input<string>('');
  iconPosition = input<'start' | 'end'>('start');
  disabled = input<boolean>(false);
  fullWidth = input<boolean>(false);
  type = input<'button' | 'submit' | 'reset'>('button');

  clicked = output<void>();

  onClick(): void {
    if (!this.disabled()) {
      this.clicked.emit();
    }
  }
}
