import { Component, output, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-global-header',
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule
  ],
  templateUrl: './global-header.html',
  styleUrl: './global-header.scss'
})
export class GlobalHeader {
  title = input<string>('Endpoint Engineering');
  notificationCount = input<number>(0);

  menuToggle = output<void>();
  searchClick = output<void>();
  notificationsClick = output<void>();
  accountClick = output<void>();

  onMenuToggle(): void {
    this.menuToggle.emit();
  }

  onSearchClick(): void {
    this.searchClick.emit();
  }

  onNotificationsClick(): void {
    this.notificationsClick.emit();
  }

  onAccountClick(): void {
    this.accountClick.emit();
  }
}
