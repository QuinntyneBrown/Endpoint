import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface FolderMapping {
  id: string;
  sourcePath: string;
  destinationPath: string;
  rootPath?: string;
}

@Component({
  selector: 'ee-folder-mapping-list',
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './folder-mapping-list.html',
  styleUrl: './folder-mapping-list.scss'
})
export class FolderMappingList {
  folders = input<FolderMapping[]>([]);
  repositoryName = input<string>('');
  editable = input<boolean>(true);

  foldersChange = output<FolderMapping[]>();
  add = output<void>();
  edit = output<FolderMapping>();
  delete = output<FolderMapping>();

  protected editingId = signal<string | null>(null);

  onDrop(event: CdkDragDrop<FolderMapping[]>): void {
    const folders = [...this.folders()];
    moveItemInArray(folders, event.previousIndex, event.currentIndex);
    this.foldersChange.emit(folders);
  }

  onAdd(): void {
    this.add.emit();
  }

  onEdit(folder: FolderMapping): void {
    this.editingId.set(folder.id);
    this.edit.emit(folder);
  }

  onDelete(folder: FolderMapping): void {
    this.delete.emit(folder);
  }

  onSourcePathChange(folder: FolderMapping, value: string): void {
    this.updateFolder(folder, { sourcePath: value });
  }

  onDestinationPathChange(folder: FolderMapping, value: string): void {
    this.updateFolder(folder, { destinationPath: value });
  }

  onRootPathChange(folder: FolderMapping, value: string): void {
    this.updateFolder(folder, { rootPath: value });
  }

  isEditing(folder: FolderMapping): boolean {
    return this.editingId() === folder.id;
  }

  finishEditing(): void {
    this.editingId.set(null);
  }

  private updateFolder(folder: FolderMapping, updates: Partial<FolderMapping>): void {
    const folders = this.folders().map(f =>
      f.id === folder.id ? { ...f, ...updates } : f
    );
    this.foldersChange.emit(folders);
  }
}
