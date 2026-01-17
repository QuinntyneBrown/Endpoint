import type { Meta, StoryObj } from '@storybook/angular';
import { Button } from './button';
import { MatIconModule } from '@angular/material/icon';

const meta: Meta<Button> = {
  title: 'Components/Button',
  component: Button,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'danger', 'icon'],
      description: 'Button visual style variant (wraps Angular Material button)',
    },
    size: {
      control: 'select',
      options: ['small', 'medium', 'large'],
      description: 'Button size',
    },
    disabled: {
      control: 'boolean',
      description: 'Disabled state',
    },
    fullWidth: {
      control: 'boolean',
      description: 'Full width button',
    },
    type: {
      control: 'select',
      options: ['button', 'submit', 'reset'],
      description: 'HTML button type',
    },
  },
};

export default meta;
type Story = StoryObj<Button>;

export const Primary: Story = {
  args: {
    variant: 'primary',
    size: 'medium',
    disabled: false,
    fullWidth: false,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled" [fullWidth]="fullWidth">Primary Button</ep-button>',
  }),
};

export const Secondary: Story = {
  args: {
    variant: 'secondary',
    size: 'medium',
    disabled: false,
    fullWidth: false,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled" [fullWidth]="fullWidth">Secondary Button</ep-button>',
  }),
};

export const Danger: Story = {
  args: {
    variant: 'danger',
    size: 'medium',
    disabled: false,
    fullWidth: false,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled" [fullWidth]="fullWidth">Delete</ep-button>',
  }),
};

export const Icon: Story = {
  args: {
    variant: 'icon',
    size: 'medium',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    moduleMetadata: {
      imports: [MatIconModule],
    },
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled"><mat-icon>settings</mat-icon></ep-button>',
  }),
};

export const Small: Story = {
  args: {
    variant: 'primary',
    size: 'small',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled">Small Button</ep-button>',
  }),
};

export const Large: Story = {
  args: {
    variant: 'primary',
    size: 'large',
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled">Large Button</ep-button>',
  }),
};

export const Disabled: Story = {
  args: {
    variant: 'primary',
    size: 'medium',
    disabled: true,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled">Disabled Button</ep-button>',
  }),
};

export const FullWidth: Story = {
  args: {
    variant: 'primary',
    size: 'medium',
    disabled: false,
    fullWidth: true,
  },
  render: (args) => ({
    props: args,
    template:
      '<ep-button [variant]="variant" [size]="size" [disabled]="disabled" [fullWidth]="fullWidth">Full Width Button</ep-button>',
  }),
};
