import type { Meta, StoryObj } from '@storybook/angular';
import { Badge } from './badge';

const meta: Meta<Badge> = {
  title: 'Components/Badge',
  component: Badge,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Badge label text',
    },
    variant: {
      control: 'select',
      options: ['success', 'processing', 'pending', 'error'],
      description: 'Badge style variant',
    },
  },
};

export default meta;
type Story = StoryObj<Badge>;

export const Success: Story = {
  args: {
    label: 'Completed',
    variant: 'success',
  },
};

export const Processing: Story = {
  args: {
    label: 'Processing',
    variant: 'processing',
  },
};

export const Pending: Story = {
  args: {
    label: 'Pending',
    variant: 'pending',
  },
};

export const Error: Story = {
  args: {
    label: 'Failed',
    variant: 'error',
  },
};

export const AllVariants: Story = {
  render: () => ({
    template: `
      <div style="display: flex; gap: 12px; flex-wrap: wrap;">
        <ee-badge label="Completed" variant="success"></ee-badge>
        <ee-badge label="Processing" variant="processing"></ee-badge>
        <ee-badge label="Pending" variant="pending"></ee-badge>
        <ee-badge label="Failed" variant="error"></ee-badge>
      </div>
    `,
  }),
};
