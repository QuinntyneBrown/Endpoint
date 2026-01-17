import type { Meta, StoryObj } from '@storybook/angular';
import { StatusIndicator } from './status-indicator';

const meta: Meta<StatusIndicator> = {
  title: 'Components/StatusIndicator',
  component: StatusIndicator,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<StatusIndicator>;

export const Success: Story = {
  args: {
    status: 'success',
    text: 'Completed successfully',
    showIcon: true,
  },
};

export const Error: Story = {
  args: {
    status: 'error',
    text: 'Failed to process',
    showIcon: true,
  },
};

export const Warning: Story = {
  args: {
    status: 'warning',
    text: 'Please review before continuing',
    showIcon: true,
  },
};

export const Info: Story = {
  args: {
    status: 'info',
    text: 'Additional information available',
    showIcon: true,
  },
};

export const Loading: Story = {
  args: {
    status: 'loading',
    text: 'Processing...',
    showIcon: true,
  },
};

export const WithoutIcon: Story = {
  args: {
    status: 'success',
    text: 'Done',
    showIcon: false,
  },
};
