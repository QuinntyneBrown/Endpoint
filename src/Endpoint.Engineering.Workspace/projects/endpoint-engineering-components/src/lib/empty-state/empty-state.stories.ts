import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { EmptyState } from './empty-state';
import { Button } from '../button';

const meta: Meta<EmptyState> = {
  title: 'Components/EmptyState',
  component: EmptyState,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [Button],
    }),
  ],
  argTypes: {
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    title: {
      control: 'text',
      description: 'Main title text',
    },
    description: {
      control: 'text',
      description: 'Description text',
    },
  },
};

export default meta;
type Story = StoryObj<EmptyState>;

export const Default: Story = {
  args: {
    icon: 'layers',
    title: 'Start Composing Your Solution',
    description:
      'Create a custom solution by selecting components from repositories, local folders, or templates. Build exactly what you need with drag-and-drop simplicity.',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-empty-state [icon]="icon" [title]="title" [description]="description">
        <div actions>
          <ee-button icon="add_circle_outline">New Composition</ee-button>
          <ee-button variant="secondary" icon="folder_open">Load Saved Configuration</ee-button>
        </div>
      </ee-empty-state>
    `,
  }),
};

export const NoResults: Story = {
  args: {
    icon: 'search_off',
    title: 'No Results Found',
    description: 'Try adjusting your search or filter criteria.',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-empty-state [icon]="icon" [title]="title" [description]="description">
        <div actions>
          <ee-button variant="secondary">Clear Filters</ee-button>
        </div>
      </ee-empty-state>
    `,
  }),
};

export const EmptyFolder: Story = {
  args: {
    icon: 'folder_open',
    title: 'This Folder is Empty',
    description: 'Add files or folders to get started.',
  },
};

export const Error: Story = {
  args: {
    icon: 'error_outline',
    title: 'Something Went Wrong',
    description:
      'We encountered an error while loading your data. Please try again.',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-empty-state [icon]="icon" [title]="title" [description]="description">
        <div actions>
          <ee-button icon="refresh">Retry</ee-button>
        </div>
      </ee-empty-state>
    `,
  }),
};
