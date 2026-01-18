import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';

export interface RailItem {
  id: string;
  icon: string;
  label: string;
  route?: string;
  badge?: number;
  action?: () => void;
}

@Component({
  selector: 'app-rail',
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatTooltipModule,
    MatBadgeModule
  ],
  templateUrl: './rail.html',
  styleUrl: './rail.scss'
})
export class Rail {
  isOpen = input<boolean>(false);

  itemClick = output<RailItem>();
  settingsClick = output<void>();

  readonly navItems: RailItem[] = [
    { id: 'home', icon: 'home', label: 'Home', route: '/home' },
    { id: 'composer', icon: 'layers', label: 'Composer', route: '/request/create' },
    { id: 'requests', icon: 'list_alt', label: 'Requests', route: '/requests' },
    { id: 'explorer', icon: 'folder', label: 'Explorer', route: '/explorer' },
    { id: 'search', icon: 'search', label: 'Search', route: '/search' },
  ];

  readonly footerItems: RailItem[] = [
    { id: 'account', icon: 'account_circle', label: 'Account' },
    { id: 'settings', icon: 'settings', label: 'Settings', route: '/settings' },
  ];

  onItemClick(item: RailItem): void {
    if (item.action) {
      item.action();
    }
    this.itemClick.emit(item);
  }

  onSettingsClick(): void {
    this.settingsClick.emit();
  }
}
