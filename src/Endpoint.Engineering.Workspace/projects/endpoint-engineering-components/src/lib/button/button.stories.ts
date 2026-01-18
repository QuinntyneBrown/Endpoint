import type { Meta, StoryObj } from '@storybook/angular';
import { Button } from './button';

const meta: Meta<Button> = {
  title: 'Components/Button',
  component: Button,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'danger'],
      description: 'Button style variant',
    },
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    iconPosition: {
      control: 'select',
      options: ['start', 'end'],
      description: 'Position of the icon',
    },
    disabled: {
      control: 'boolean',
      description: 'Whether the button is disabled',
    },
    fullWidth: {
      control: 'boolean',
      description: 'Whether the button takes full width',
    },
  },
  render: (args) => ({
    props: args,
    template: `<ee-button
      [variant]="variant"
      [icon]="icon"
      [iconPosition]="iconPosition"
      [disabled]="disabled"
      [fullWidth]="fullWidth"
    >Button Text</ee-button>`,
  }),
};

export default meta;
type Story = StoryObj<Button>;

export const Primary: Story = {
  args: {
    variant: 'primary',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant">New Composition</ee-button>`,
  }),
};

export const Secondary: Story = {
  args: {
    variant: 'secondary',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant">Cancel</ee-button>`,
  }),
};

export const Danger: Story = {
  args: {
    variant: 'danger',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant">Delete</ee-button>`,
  }),
};

export const WithIconStart: Story = {
  args: {
    variant: 'primary',
    icon: 'add_circle_outline',
    iconPosition: 'start',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant" [icon]="icon" [iconPosition]="iconPosition">New Composition</ee-button>`,
  }),
};

export const WithIconEnd: Story = {
  args: {
    variant: 'primary',
    icon: 'arrow_forward',
    iconPosition: 'end',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant" [icon]="icon" [iconPosition]="iconPosition">Next Step</ee-button>`,
  }),
};

export const Disabled: Story = {
  args: {
    variant: 'primary',
    disabled: true,
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant" [disabled]="disabled">Disabled Button</ee-button>`,
  }),
};

export const FullWidth: Story = {
  args: {
    variant: 'primary',
    fullWidth: true,
    icon: 'add',
  },
  render: (args) => ({
    props: args,
    template: `<ee-button [variant]="variant" [fullWidth]="fullWidth" [icon]="icon">Full Width Button</ee-button>`,
  }),
};
