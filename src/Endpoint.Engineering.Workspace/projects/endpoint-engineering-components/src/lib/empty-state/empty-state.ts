import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ee-empty-state',
  imports: [CommonModule],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
})
export class EmptyState {
  icon = input<string>('layers');
  title = input.required<string>();
  description = input<string>('');
}
