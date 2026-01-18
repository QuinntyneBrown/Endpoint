import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface StatItem {
  value: number | string;
  label: string;
}

@Component({
  selector: 'ee-stats-bar',
  imports: [CommonModule],
  templateUrl: './stats-bar.html',
  styleUrl: './stats-bar.scss',
})
export class StatsBar {
  stats = input.required<StatItem[]>();
}
