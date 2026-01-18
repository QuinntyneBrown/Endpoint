import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type TagVariant = 'default' | 'github' | 'local' | 'primary';

@Component({
  selector: 'ee-tag',
  imports: [CommonModule],
  templateUrl: './tag.html',
  styleUrl: './tag.scss',
})
export class Tag {
  label = input.required<string>();
  variant = input<TagVariant>('default');
  removable = input<boolean>(false);

  removed = output<void>();

  onRemove(event: Event): void {
    event.stopPropagation();
    this.removed.emit();
  }
}
