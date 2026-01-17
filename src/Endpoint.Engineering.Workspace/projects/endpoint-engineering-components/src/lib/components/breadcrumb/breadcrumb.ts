import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BreadcrumbItem {
  label: string;
  url?: string;
}

@Component({
  selector: 'ep-breadcrumb',
  imports: [CommonModule],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.scss',
})
export class Breadcrumb {
  @Input() items: BreadcrumbItem[] = [];
  @Output() itemClick = new EventEmitter<BreadcrumbItem>();

  onItemClick(item: BreadcrumbItem, index: number): void {
    if (index < this.items.length - 1) {
      this.itemClick.emit(item);
    }
  }

  isLast(index: number): boolean {
    return index === this.items.length - 1;
  }
}
