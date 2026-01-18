import type { Meta, StoryObj } from '@storybook/angular';
import { PageHeader } from './page-header';

const meta: Meta<PageHeader> = {
  title: 'Components/PageHeader',
  component: PageHeader,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Header title text',
    },
    showBackButton: {
      control: 'boolean',
      description: 'Show the back navigation button',
    },
    showHelpButton: {
      control: 'boolean',
      description: 'Show the help button',
    },
    showMenuButton: {
      control: 'boolean',
      description: 'Show the menu button',
    },
  },
};

export default meta;
type Story = StoryObj<PageHeader>;

export const Default: Story = {
  args: {
    title: 'A La Carte Composer',
    showBackButton: true,
    showHelpButton: true,
    showMenuButton: true,
  },
};

export const SavedConfigurations: Story = {
  args: {
    title: 'Saved Configurations',
    showBackButton: true,
    showHelpButton: true,
    showMenuButton: true,
  },
};

export const ExecutingRequest: Story = {
  args: {
    title: 'Executing Request',
    showBackButton: true,
    showHelpButton: true,
    showMenuButton: false,
  },
};

export const NoBackButton: Story = {
  args: {
    title: 'Dashboard',
    showBackButton: false,
    showHelpButton: true,
    showMenuButton: true,
  },
};

export const MinimalHeader: Story = {
  args: {
    title: 'Simple Page',
    showBackButton: false,
    showHelpButton: false,
    showMenuButton: false,
  },
};
