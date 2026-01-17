import type { Meta, StoryObj } from '@storybook/angular';
import { Card } from './card';

const meta: Meta<Card> = {
  title: 'Components/Card',
  component: Card,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['default', 'elevated', 'outlined'],
      description: 'Card visual style variant',
    },
    padding: {
      control: 'select',
      options: ['none', 'small', 'medium', 'large'],
      description: 'Card padding size',
    },
    hoverable: {
      control: 'boolean',
      description: 'Enable hover effect',
    },
  },
};

export default meta;
type Story = StoryObj<Card>;

export const Default: Story = {
  args: {
    variant: 'default',
    padding: 'medium',
    hoverable: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <h3 style="margin: 0 0 8px 0; color: #e6edf3;">Card Title</h3>
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">This is a card component with default styling. It can contain any content.</p>
      </ep-card>
    `,
  }),
};

export const Elevated: Story = {
  args: {
    variant: 'elevated',
    padding: 'medium',
    hoverable: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <h3 style="margin: 0 0 8px 0; color: #e6edf3;">Elevated Card</h3>
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">This card has an elevated appearance with shadow.</p>
      </ep-card>
    `,
  }),
};

export const Outlined: Story = {
  args: {
    variant: 'outlined',
    padding: 'medium',
    hoverable: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <h3 style="margin: 0 0 8px 0; color: #e6edf3;">Outlined Card</h3>
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">This card has an outlined border style.</p>
      </ep-card>
    `,
  }),
};

export const Hoverable: Story = {
  args: {
    variant: 'default',
    padding: 'medium',
    hoverable: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <h3 style="margin: 0 0 8px 0; color: #e6edf3;">Hoverable Card</h3>
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">Hover over this card to see the effect.</p>
      </ep-card>
    `,
  }),
};

export const SmallPadding: Story = {
  args: {
    variant: 'default',
    padding: 'small',
    hoverable: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <p style="margin: 0; color: #e6edf3;">Small padding card</p>
      </ep-card>
    `,
  }),
};

export const LargePadding: Story = {
  args: {
    variant: 'default',
    padding: 'large',
    hoverable: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-card [variant]="variant" [padding]="padding" [hoverable]="hoverable">
        <h3 style="margin: 0 0 12px 0; color: #e6edf3;">Large Padding Card</h3>
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">This card has large padding for more spacious content.</p>
      </ep-card>
    `,
  }),
};
