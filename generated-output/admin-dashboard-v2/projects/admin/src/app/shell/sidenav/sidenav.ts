import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'admin-sidenav',
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss'
})
export class Sidenav {
  navItems: NavItem[] = [
    { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
    { icon: 'people', label: 'Users', route: '/users' },
    { icon: 'admin_panel_settings', label: 'Roles', route: '/roles' },
    { icon: 'settings', label: 'Settings', route: '/settings' }
  ];
}
