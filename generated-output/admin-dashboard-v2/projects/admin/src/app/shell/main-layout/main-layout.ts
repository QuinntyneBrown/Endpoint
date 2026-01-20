import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { GlobalHeader } from '../global-header/global-header';
import { Sidenav } from '../sidenav/sidenav';

@Component({
  selector: 'admin-main-layout',
  imports: [CommonModule, RouterOutlet, GlobalHeader, Sidenav],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {
}
