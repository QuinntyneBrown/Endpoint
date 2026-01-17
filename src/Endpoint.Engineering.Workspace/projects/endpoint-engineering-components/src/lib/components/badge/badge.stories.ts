import type { Meta, StoryObj } from '@storybook/angular';
import { Badge } from './badge';

const meta: Meta<Badge> = {
  title: 'Components/Badge',
  component: Badge,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'success', 'warning', 'danger', 'neutral'],
      description: 'Badge color variant',
    },
    size: {
      control: 'select',
      options: ['small', 'medium'],
      description: 'Badge size',
    },
    dot: {
      control: 'boolean',
      description: 'Display as a small dot indicator',
    },
  },
};

export default meta;
type Story = StoryObj<Badge>;

export const Primary: Story = {
  args: {
    variant: 'primary',
    size: 'medium',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Primary</ep-badge>',
  }),
};

export const Success: Story = {
  args: {
    variant: 'success',
    size: 'medium',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Success</ep-badge>',
  }),
};

export const Warning: Story = {
  args: {
    variant: 'warning',
    size: 'medium',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Warning</ep-badge>',
  }),
};

export const Danger: Story = {
  args: {
    variant: 'danger',
    size: 'medium',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Danger</ep-badge>',
  }),
};

export const Neutral: Story = {
  args: {
    variant: 'neutral',
    size: 'medium',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Neutral</ep-badge>',
  }),
};

export const Small: Story = {
  args: {
    variant: 'primary',
    size: 'small',
    dot: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-badge [variant]="variant" [size]="size" [dot]="dot">Small</ep-badge>',
  }),
};

export const Dot: Story = {
  args: {
    variant: 'success',
    size: 'medium',
    dot: true,
  },
  render: (args) => ({
    props: args,
    template: '<div style="display: flex; gap: 12px; align-items: center;"><ep-badge [variant]="variant" [size]="size" [dot]="dot"></ep-badge><span style="color: #e6edf3;">Online</span></div>',
  }),
};

export const AllVariants: Story = {
  render: () => ({
    template: `
      <div style="display: flex; gap: 8px; flex-wrap: wrap;">
        <ep-badge variant="primary">Primary</ep-badge>
        <ep-badge variant="success">Success</ep-badge>
        <ep-badge variant="warning">Warning</ep-badge>
        <ep-badge variant="danger">Danger</ep-badge>
        <ep-badge variant="neutral">Neutral</ep-badge>
      </div>
    `,
  }),
};
