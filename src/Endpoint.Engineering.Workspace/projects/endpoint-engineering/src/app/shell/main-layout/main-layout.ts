import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { GlobalHeader } from '../global-header/global-header';
import { Rail } from '../rail/rail';

@Component({
  selector: 'app-main-layout',
  imports: [
    CommonModule,
    RouterOutlet,
    GlobalHeader,
    Rail
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {
  protected readonly mobileMenuOpen = signal(false);
  protected readonly notificationCount = signal(0);

  onMenuToggle(): void {
    this.mobileMenuOpen.update(open => !open);
  }

  onSearchClick(): void {
    // TODO: Implement search dialog
    console.log('Search clicked');
  }

  onNotificationsClick(): void {
    // TODO: Implement notifications panel
    console.log('Notifications clicked');
  }

  onAccountClick(): void {
    // TODO: Implement account menu
    console.log('Account clicked');
  }

  onSettingsClick(): void {
    // TODO: Navigate to settings
    console.log('Settings clicked');
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }
}
