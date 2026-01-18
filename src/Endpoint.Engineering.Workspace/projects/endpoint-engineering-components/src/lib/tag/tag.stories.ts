import type { Meta, StoryObj } from '@storybook/angular';
import { Tag } from './tag';

const meta: Meta<Tag> = {
  title: 'Components/Tag',
  component: Tag,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Tag label text',
    },
    variant: {
      control: 'select',
      options: ['default', 'github', 'local', 'primary'],
      description: 'Tag style variant',
    },
    removable: {
      control: 'boolean',
      description: 'Show remove button',
    },
  },
};

export default meta;
type Story = StoryObj<Tag>;

export const Default: Story = {
  args: {
    label: '.NET 8',
    variant: 'default',
    removable: false,
  },
};

export const GitHub: Story = {
  args: {
    label: 'GitHub',
    variant: 'github',
  },
};

export const Local: Story = {
  args: {
    label: 'Local',
    variant: 'local',
  },
};

export const Primary: Story = {
  args: {
    label: 'api',
    variant: 'primary',
    removable: true,
  },
};

export const Removable: Story = {
  args: {
    label: 'microservice',
    variant: 'primary',
    removable: true,
  },
};

export const AllVariants: Story = {
  render: () => ({
    template: `
      <div style="display: flex; gap: 8px; flex-wrap: wrap;">
        <ee-tag label=".NET 8"></ee-tag>
        <ee-tag label="GitHub" variant="github"></ee-tag>
        <ee-tag label="Local" variant="local"></ee-tag>
        <ee-tag label="api" variant="primary" [removable]="true"></ee-tag>
        <ee-tag label="Docker"></ee-tag>
      </div>
    `,
  }),
};
