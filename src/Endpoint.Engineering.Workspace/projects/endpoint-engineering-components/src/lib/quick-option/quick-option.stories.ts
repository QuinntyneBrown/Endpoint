import type { Meta, StoryObj } from '@storybook/angular';
import { QuickOption } from './quick-option';

const meta: Meta<QuickOption> = {
  title: 'Components/QuickOption',
  component: QuickOption,
  tags: ['autodocs'],
  argTypes: {
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    title: {
      control: 'text',
      description: 'Option title',
    },
    description: {
      control: 'text',
      description: 'Option description',
    },
  },
};

export default meta;
type Story = StoryObj<QuickOption>;

export const FromGitHub: Story = {
  args: {
    icon: 'cloud',
    title: 'From GitHub',
    description: 'Browse repository folders',
  },
};

export const FromLocal: Story = {
  args: {
    icon: 'folder',
    title: 'From Local',
    description: 'Select local directories',
  },
};

export const Recent: Story = {
  args: {
    icon: 'history',
    title: 'Recent',
    description: 'View recent compositions',
  },
};

export const Templates: Story = {
  args: {
    icon: 'dashboard',
    title: 'Templates',
    description: 'Use pre-built templates',
  },
};

export const NoDescription: Story = {
  args: {
    icon: 'settings',
    title: 'Settings',
  },
};
