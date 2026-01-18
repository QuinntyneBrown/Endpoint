import type { Meta, StoryObj } from '@storybook/angular';
import { IconButton } from './icon-button';

const meta: Meta<IconButton> = {
  title: 'Components/IconButton',
  component: IconButton,
  tags: ['autodocs'],
  argTypes: {
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    variant: {
      control: 'select',
      options: ['default', 'back'],
      description: 'Button variant style',
    },
    disabled: {
      control: 'boolean',
      description: 'Whether the button is disabled',
    },
    ariaLabel: {
      control: 'text',
      description: 'Accessibility label for the button',
    },
  },
};

export default meta;
type Story = StoryObj<IconButton>;

export const Default: Story = {
  args: {
    icon: 'more_vert',
    variant: 'default',
    disabled: false,
  },
};

export const BackButton: Story = {
  args: {
    icon: 'arrow_back',
    variant: 'back',
    ariaLabel: 'Go back',
  },
};

export const HelpButton: Story = {
  args: {
    icon: 'help_outline',
    ariaLabel: 'Get help',
  },
};

export const CloseButton: Story = {
  args: {
    icon: 'close',
    ariaLabel: 'Close',
  },
};

export const Disabled: Story = {
  args: {
    icon: 'settings',
    disabled: true,
  },
};
