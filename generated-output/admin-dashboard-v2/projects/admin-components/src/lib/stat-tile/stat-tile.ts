import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'admin-stat-tile',
  imports: [CommonModule, MatIconModule],
  templateUrl: './stat-tile.html',
  styleUrl: './stat-tile.scss'
})
export class StatTile {
  @Input() title = '';
  @Input() value = '';
  @Input() icon = 'analytics';
  @Input() trend: 'up' | 'down' | 'neutral' = 'neutral';
}
