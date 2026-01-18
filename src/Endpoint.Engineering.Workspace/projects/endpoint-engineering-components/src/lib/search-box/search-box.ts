import { Component, input, output, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'ee-search-box',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-box.html',
  styleUrl: './search-box.scss',
})
export class SearchBox {
  placeholder = input<string>('Search...');
  value = model<string>('');

  searchChanged = output<string>();

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value.set(target.value);
    this.searchChanged.emit(target.value);
  }

  clear(): void {
    this.value.set('');
    this.searchChanged.emit('');
  }
}
