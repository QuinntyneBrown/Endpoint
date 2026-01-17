import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ep-dialog',
  imports: [CommonModule],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {
  @Input() open: boolean = false;
  @Input() title: string = '';
  @Input() width: 'small' | 'medium' | 'large' = 'medium';
  @Output() close = new EventEmitter<void>();

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }

  onClose(): void {
    this.close.emit();
  }

  get dialogClasses(): string {
    const classes = ['ep-dialog'];
    classes.push(`ep-dialog--${this.width}`);
    return classes.join(' ');
  }
}
