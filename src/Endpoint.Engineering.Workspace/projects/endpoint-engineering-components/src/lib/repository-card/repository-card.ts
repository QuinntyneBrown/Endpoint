import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface RepositoryData {
  id: string;
  url: string;
  branch: string;
  isLocal: boolean;
  localDirectory?: string;
  foldersCount?: number;
}

@Component({
  selector: 'ee-repository-card',
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './repository-card.html',
  styleUrl: './repository-card.scss'
})
export class RepositoryCard {
  repository = input.required<RepositoryData>();
  editable = input<boolean>(true);
  expanded = input<boolean>(false);

  edit = output<RepositoryData>();
  delete = output<RepositoryData>();
  urlChange = output<string>();
  branchChange = output<string>();
  localDirectoryChange = output<string>();

  onEdit(): void {
    this.edit.emit(this.repository());
  }

  onDelete(): void {
    this.delete.emit(this.repository());
  }

  onUrlChange(value: string): void {
    this.urlChange.emit(value);
  }

  onBranchChange(value: string): void {
    this.branchChange.emit(value);
  }

  onLocalDirectoryChange(value: string): void {
    this.localDirectoryChange.emit(value);
  }

  getDisplayName(): string {
    const repo = this.repository();
    if (repo.isLocal && repo.localDirectory) {
      return repo.localDirectory;
    }
    // Extract repo name from URL
    if (repo.url) {
      const parts = repo.url.replace(/\.git$/, '').split('/');
      return parts.slice(-2).join('/');
    }
    return 'Unknown Repository';
  }
}
