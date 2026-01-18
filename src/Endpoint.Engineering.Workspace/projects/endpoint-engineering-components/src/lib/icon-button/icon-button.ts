import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type IconButtonVariant = 'default' | 'back';

@Component({
  selector: 'ee-icon-button',
  imports: [CommonModule],
  templateUrl: './icon-button.html',
  styleUrl: './icon-button.scss',
})
export class IconButton {
  icon = input.required<string>();
  variant = input<IconButtonVariant>('default');
  disabled = input<boolean>(false);
  ariaLabel = input<string>('');

  clicked = output<void>();

  onClick(): void {
    if (!this.disabled()) {
      this.clicked.emit();
    }
  }
}
