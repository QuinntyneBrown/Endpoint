import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ee-quick-option',
  imports: [CommonModule],
  templateUrl: './quick-option.html',
  styleUrl: './quick-option.scss',
})
export class QuickOption {
  icon = input.required<string>();
  title = input.required<string>();
  description = input<string>('');

  selected = output<void>();

  onSelect(): void {
    this.selected.emit();
  }
}
