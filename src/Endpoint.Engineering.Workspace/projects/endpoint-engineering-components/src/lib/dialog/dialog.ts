import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconButton } from '../icon-button';

@Component({
  selector: 'ee-dialog',
  imports: [CommonModule, IconButton],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {
  title = input.required<string>();
  icon = input<string>('');
  showCloseButton = input<boolean>(true);
  open = input<boolean>(true);

  closed = output<void>();

  onClose(): void {
    this.closed.emit();
  }

  onOverlayClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
