import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ee-progress-bar',
  imports: [CommonModule],
  templateUrl: './progress-bar.html',
  styleUrl: './progress-bar.scss',
})
export class ProgressBar {
  value = input<number>(0);
  label = input<string>('');
  showPercentage = input<boolean>(true);
  animated = input<boolean>(true);
}
