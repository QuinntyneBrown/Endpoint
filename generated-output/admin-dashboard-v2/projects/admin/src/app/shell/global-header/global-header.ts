import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'admin-global-header',
  imports: [CommonModule, MatToolbarModule, MatIconModule, MatButtonModule, MatMenuModule],
  templateUrl: './global-header.html',
  styleUrl: './global-header.scss'
})
export class GlobalHeader {
}
