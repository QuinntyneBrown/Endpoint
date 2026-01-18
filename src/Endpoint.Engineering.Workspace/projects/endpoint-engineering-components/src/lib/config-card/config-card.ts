import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tag } from '../tag';

export interface ConfigCardMeta {
  icon: string;
  label: string;
}

export interface ConfigCardTag {
  label: string;
  variant?: 'default' | 'github' | 'local' | 'primary';
}

@Component({
  selector: 'ee-config-card',
  imports: [CommonModule, Tag],
  templateUrl: './config-card.html',
  styleUrl: './config-card.scss',
})
export class ConfigCard {
  name = input.required<string>();
  description = input<string>('');
  icon = input<string>('layers');
  lastUsed = input<string>('');
  meta = input<ConfigCardMeta[]>([]);
  tags = input<ConfigCardTag[]>([]);

  cardClicked = output<void>();
  menuClicked = output<void>();

  onCardClick(): void {
    this.cardClicked.emit();
  }

  onMenuClick(event: Event): void {
    event.stopPropagation();
    this.menuClicked.emit();
  }
}
