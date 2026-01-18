import type { Meta, StoryObj } from '@storybook/angular';
import { ProgressBar } from './progress-bar';

const meta: Meta<ProgressBar> = {
  title: 'Components/ProgressBar',
  component: ProgressBar,
  tags: ['autodocs'],
  argTypes: {
    value: {
      control: { type: 'range', min: 0, max: 100 },
      description: 'Progress value (0-100)',
    },
    label: {
      control: 'text',
      description: 'Label text',
    },
    showPercentage: {
      control: 'boolean',
      description: 'Show percentage text',
    },
    animated: {
      control: 'boolean',
      description: 'Enable pulse animation',
    },
  },
};

export default meta;
type Story = StoryObj<ProgressBar>;

export const Default: Story = {
  args: {
    value: 60,
    label: 'Overall Progress',
    showPercentage: true,
    animated: true,
  },
};

export const Starting: Story = {
  args: {
    value: 10,
    label: 'Starting...',
    animated: true,
  },
};

export const Halfway: Story = {
  args: {
    value: 50,
    label: 'Processing',
    animated: true,
  },
};

export const AlmostComplete: Story = {
  args: {
    value: 90,
    label: 'Almost there',
    animated: true,
  },
};

export const Complete: Story = {
  args: {
    value: 100,
    label: 'Complete',
    animated: false,
  },
};

export const NoLabel: Story = {
  args: {
    value: 75,
    showPercentage: true,
  },
};

export const NoPercentage: Story = {
  args: {
    value: 40,
    label: 'Loading...',
    showPercentage: false,
    animated: true,
  },
};
