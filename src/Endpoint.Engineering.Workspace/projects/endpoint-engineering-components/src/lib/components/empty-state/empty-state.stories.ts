import type { Meta, StoryObj } from '@storybook/angular';
import { EmptyState } from './empty-state';

const meta: Meta<EmptyState> = {
  title: 'Components/EmptyState',
  component: EmptyState,
  tags: ['autodocs'],
  argTypes: {
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    title: {
      control: 'text',
      description: 'Empty state title',
    },
    description: {
      control: 'text',
      description: 'Empty state description',
    },
  },
};

export default meta;
type Story = StoryObj<EmptyState>;

export const Default: Story = {
  args: {
    icon: 'inbox',
    title: 'No items found',
    description: 'There are no items to display at this time.',
  },
  render: (args) => ({
    props: args,
    template: '<ep-empty-state [icon]="icon" [title]="title" [description]="description"></ep-empty-state>',
  }),
};

export const WithAction: Story = {
  args: {
    icon: 'folder_open',
    title: 'No configurations yet',
    description: 'Get started by creating your first repository configuration.',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-empty-state [icon]="icon" [title]="title" [description]="description">
        <button style="background: #9fa8da; color: #000; padding: 12px 24px; border: none; border-radius: 4px; font-weight: 500; cursor: pointer;">
          Create Configuration
        </button>
      </ep-empty-state>
    `,
  }),
};

export const NoResults: Story = {
  args: {
    icon: 'search_off',
    title: 'No results found',
    description: 'Try adjusting your search or filters to find what you\'re looking for.',
  },
  render: (args) => ({
    props: args,
    template: '<ep-empty-state [icon]="icon" [title]="title" [description]="description"></ep-empty-state>',
  }),
};

export const Error: Story = {
  args: {
    icon: 'error_outline',
    title: 'Something went wrong',
    description: 'We couldn\'t load the content. Please try again.',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-empty-state [icon]="icon" [title]="title" [description]="description">
        <button style="background: transparent; color: #e6edf3; padding: 12px 24px; border: 1px solid rgba(230, 237, 243, 0.12); border-radius: 4px; font-weight: 500; cursor: pointer;">
          Try Again
        </button>
      </ep-empty-state>
    `,
  }),
};
