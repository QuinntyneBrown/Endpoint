import type { Meta, StoryObj } from '@storybook/angular';
import { TreeView, TreeNode } from './tree-view';

const meta: Meta<TreeView> = {
  title: 'Components/TreeView',
  component: TreeView,
  tags: ['autodocs'],
  argTypes: {
    selectable: {
      control: 'boolean',
      description: 'Enable node selection',
    },
    expandable: {
      control: 'boolean',
      description: 'Enable expand/collapse',
    },
  },
};

export default meta;
type Story = StoryObj<TreeView>;

const sampleNodes: TreeNode[] = [
  {
    id: '1',
    label: 'src',
    icon: 'folder',
    expanded: true,
    children: [
      {
        id: '1-1',
        label: 'app',
        icon: 'folder',
        expanded: true,
        children: [
          { id: '1-1-1', label: 'app.component.ts', icon: 'code' },
          { id: '1-1-2', label: 'app.component.html', icon: 'html' },
          { id: '1-1-3', label: 'app.component.scss', icon: 'style' },
        ],
      },
      {
        id: '1-2',
        label: 'assets',
        icon: 'folder',
        children: [
          { id: '1-2-1', label: 'images', icon: 'folder' },
          { id: '1-2-2', label: 'styles.css', icon: 'style' },
        ],
      },
    ],
  },
  {
    id: '2',
    label: 'package.json',
    icon: 'description',
  },
  {
    id: '3',
    label: 'README.md',
    icon: 'description',
  },
];

export const Default: Story = {
  args: {
    nodes: sampleNodes,
    selectable: false,
    expandable: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-tree-view [nodes]="nodes" [selectable]="selectable" [expandable]="expandable"></ep-tree-view>',
  }),
};

export const Collapsed: Story = {
  args: {
    nodes: [
      {
        id: '1',
        label: 'src',
        icon: 'folder',
        expanded: false,
        children: [
          { id: '1-1', label: 'app', icon: 'folder' },
          { id: '1-2', label: 'assets', icon: 'folder' },
        ],
      },
      { id: '2', label: 'package.json', icon: 'description' },
    ],
    selectable: false,
    expandable: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-tree-view [nodes]="nodes" [selectable]="selectable" [expandable]="expandable"></ep-tree-view>',
  }),
};

export const WithSelection: Story = {
  args: {
    nodes: [
      {
        id: '1',
        label: 'src',
        icon: 'folder',
        expanded: true,
        children: [
          { id: '1-1', label: 'app', icon: 'folder', selected: true },
          { id: '1-2', label: 'assets', icon: 'folder' },
        ],
      },
    ],
    selectable: true,
    expandable: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-tree-view [nodes]="nodes" [selectable]="selectable" [expandable]="expandable"></ep-tree-view>',
  }),
};

export const FileTypes: Story = {
  args: {
    nodes: [
      { id: '1', label: 'app.component.ts', icon: 'code' },
      { id: '2', label: 'styles.scss', icon: 'style' },
      { id: '3', label: 'README.md', icon: 'description' },
      { id: '4', label: 'image.png', icon: 'image' },
      { id: '5', label: 'video.mp4', icon: 'movie' },
    ],
    selectable: false,
    expandable: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-tree-view [nodes]="nodes" [selectable]="selectable" [expandable]="expandable"></ep-tree-view>',
  }),
};
