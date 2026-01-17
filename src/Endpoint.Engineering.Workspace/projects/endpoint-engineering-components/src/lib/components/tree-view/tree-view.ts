import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TreeNode {
  id: string;
  label: string;
  icon?: string;
  children?: TreeNode[];
  expanded?: boolean;
  selected?: boolean;
  metadata?: any;
}

@Component({
  selector: 'ep-tree-view',
  imports: [CommonModule],
  templateUrl: './tree-view.html',
  styleUrl: './tree-view.scss',
})
export class TreeView {
  @Input() nodes: TreeNode[] = [];
  @Input() selectable: boolean = false;
  @Input() expandable: boolean = true;
  @Output() nodeClick = new EventEmitter<TreeNode>();
  @Output() nodeToggle = new EventEmitter<TreeNode>();

  onNodeClick(node: TreeNode, event: MouseEvent): void {
    event.stopPropagation();
    this.nodeClick.emit(node);
  }

  onToggle(node: TreeNode, event: MouseEvent): void {
    event.stopPropagation();
    if (this.expandable && node.children && node.children.length > 0) {
      node.expanded = !node.expanded;
      this.nodeToggle.emit(node);
    }
  }

  hasChildren(node: TreeNode): boolean {
    return !!node.children && node.children.length > 0;
  }
}
