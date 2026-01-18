import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconButton } from '../icon-button';

@Component({
  selector: 'ee-page-header',
  imports: [CommonModule, IconButton],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss',
})
export class PageHeader {
  title = input.required<string>();
  showBackButton = input<boolean>(true);
  showHelpButton = input<boolean>(true);
  showMenuButton = input<boolean>(true);

  backClicked = output<void>();
  helpClicked = output<void>();
  menuClicked = output<void>();

  onBackClick(): void {
    this.backClicked.emit();
  }

  onHelpClick(): void {
    this.helpClicked.emit();
  }

  onMenuClick(): void {
    this.menuClicked.emit();
  }
}
