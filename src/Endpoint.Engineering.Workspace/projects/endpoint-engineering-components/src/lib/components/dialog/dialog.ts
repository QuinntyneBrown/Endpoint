import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ep-dialog',
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {
  @Input() open: boolean = false;
  @Input() title: string = '';
  @Input() width: 'small' | 'medium' | 'large' = 'medium';
  @Output() dialogClose = new EventEmitter<void>();

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }

  onClose(): void {
    this.dialogClose.emit();
  }

  get dialogClasses(): string {
    const classes = ['ep-dialog'];
    classes.push(`ep-dialog--${this.width}`);
    return classes.join(' ');
  }
}
