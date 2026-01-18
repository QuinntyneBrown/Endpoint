import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ee-json-preview',
  imports: [CommonModule, MatIconModule],
  templateUrl: './json-preview.html',
  styleUrl: './json-preview.scss'
})
export class JsonPreview {
  data = input.required<object>();
  title = input<string>('JSON Preview');
  maxHeight = input<string>('400px');
  showCopyButton = input<boolean>(true);

  protected copied = false;

  protected formattedJson = computed(() => {
    return this.syntaxHighlight(JSON.stringify(this.data(), null, 2));
  });

  async copyToClipboard(): Promise<void> {
    try {
      await navigator.clipboard.writeText(JSON.stringify(this.data(), null, 2));
      this.copied = true;
      setTimeout(() => {
        this.copied = false;
      }, 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  }

  private syntaxHighlight(json: string): string {
    // Replace special characters to prevent XSS
    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    // Apply syntax highlighting
    return json.replace(
      /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
      (match) => {
        let cls = 'number';
        if (/^"/.test(match)) {
          if (/:$/.test(match)) {
            cls = 'key';
          } else {
            cls = 'string';
          }
        } else if (/true|false/.test(match)) {
          cls = 'boolean';
        } else if (/null/.test(match)) {
          cls = 'null';
        }
        return `<span class="json-${cls}">${match}</span>`;
      }
    );
  }
}
