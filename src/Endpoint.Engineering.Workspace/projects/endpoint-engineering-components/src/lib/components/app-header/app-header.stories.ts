import type { Meta, StoryObj } from '@storybook/angular';
import { AppHeader } from './app-header';

const meta: Meta<AppHeader> = {
  title: 'Components/AppHeader',
  component: AppHeader,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Header title text',
    },
    showBackButton: {
      control: 'boolean',
      description: 'Show back navigation button',
    },
  },
};

export default meta;
type Story = StoryObj<AppHeader>;

export const Default: Story = {
  args: {
    title: 'Solution Composer',
    showBackButton: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-app-header [title]="title" [showBackButton]="showBackButton">
        <button style="background: none; border: none; color: rgba(230, 237, 243, 0.7); cursor: pointer; padding: 8px; border-radius: 50%; display: flex;">
          <span class="material-icons">settings</span>
        </button>
        <button style="background: none; border: none; color: rgba(230, 237, 243, 0.7); cursor: pointer; padding: 8px; border-radius: 50%; display: flex;">
          <span class="material-icons">more_vert</span>
        </button>
      </ep-app-header>
    `,
  }),
};

export const WithBackButton: Story = {
  args: {
    title: 'Repository Configuration',
    showBackButton: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-app-header [title]="title" [showBackButton]="showBackButton"></ep-app-header>',
  }),
};

export const WithActions: Story = {
  args: {
    title: 'Requests',
    showBackButton: false,
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-app-header [title]="title" [showBackButton]="showBackButton">
        <button style="background: none; border: none; color: rgba(230, 237, 243, 0.7); cursor: pointer; padding: 8px; border-radius: 50%; display: flex;">
          <span class="material-icons">search</span>
        </button>
        <button style="background: none; border: none; color: rgba(230, 237, 243, 0.7); cursor: pointer; padding: 8px; border-radius: 50%; display: flex;">
          <span class="material-icons">filter_list</span>
        </button>
        <button style="background: none; border: none; color: rgba(230, 237, 243, 0.7); cursor: pointer; padding: 8px; border-radius: 50%; display: flex;">
          <span class="material-icons">more_vert</span>
        </button>
      </ep-app-header>
    `,
  }),
};
