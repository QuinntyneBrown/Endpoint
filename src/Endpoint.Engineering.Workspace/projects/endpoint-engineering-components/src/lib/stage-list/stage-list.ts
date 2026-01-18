import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Badge, BadgeVariant } from '../badge';

export type StageStatus = 'pending' | 'active' | 'completed' | 'error';

export interface Stage {
  name: string;
  status: StageStatus;
  description?: string;
}

@Component({
  selector: 'ee-stage-list',
  imports: [CommonModule, Badge],
  templateUrl: './stage-list.html',
  styleUrl: './stage-list.scss',
})
export class StageList {
  stages = input.required<Stage[]>();

  getIconForStatus(status: StageStatus): string {
    switch (status) {
      case 'completed':
        return 'check_circle';
      case 'active':
        return 'hourglass_empty';
      case 'error':
        return 'error';
      default:
        return 'radio_button_unchecked';
    }
  }

  getBadgeVariant(status: StageStatus): BadgeVariant {
    switch (status) {
      case 'completed':
        return 'success';
      case 'active':
        return 'processing';
      case 'error':
        return 'error';
      default:
        return 'pending';
    }
  }

  getBadgeLabel(status: StageStatus): string {
    switch (status) {
      case 'completed':
        return 'Completed';
      case 'active':
        return 'Processing';
      case 'error':
        return 'Failed';
      default:
        return 'Pending';
    }
  }
}
