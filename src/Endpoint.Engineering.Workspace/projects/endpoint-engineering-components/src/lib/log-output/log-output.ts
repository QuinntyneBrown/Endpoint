import { Component, input, ElementRef, viewChild, effect } from '@angular/core';
import { CommonModule } from '@angular/common';

export type LogLevel = 'info' | 'success' | 'warning' | 'error';

export interface LogEntry {
  level: LogLevel;
  message: string;
  timestamp?: Date;
}

@Component({
  selector: 'ee-log-output',
  imports: [CommonModule],
  templateUrl: './log-output.html',
  styleUrl: './log-output.scss',
})
export class LogOutput {
  logs = input.required<LogEntry[]>();
  title = input<string>('');
  maxHeight = input<string>('400px');
  autoScroll = input<boolean>(true);

  logContainer = viewChild<ElementRef>('logContainer');

  constructor() {
    effect(() => {
      const logs = this.logs();
      const container = this.logContainer();
      if (this.autoScroll() && container && logs.length > 0) {
        setTimeout(() => {
          container.nativeElement.scrollTop = container.nativeElement.scrollHeight;
        });
      }
    });
  }

  getLevelPrefix(level: LogLevel): string {
    switch (level) {
      case 'info':
        return '[INFO]';
      case 'success':
        return '[SUCCESS]';
      case 'warning':
        return '[WARNING]';
      case 'error':
        return '[ERROR]';
    }
  }
}
