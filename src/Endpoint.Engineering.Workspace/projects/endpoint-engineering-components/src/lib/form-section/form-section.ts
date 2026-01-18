import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ee-form-section',
  imports: [CommonModule],
  templateUrl: './form-section.html',
  styleUrl: './form-section.scss',
})
export class FormSection {
  title = input<string>('');
}
