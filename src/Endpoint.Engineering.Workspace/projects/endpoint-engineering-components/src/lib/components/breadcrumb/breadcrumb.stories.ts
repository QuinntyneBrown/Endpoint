import type { Meta, StoryObj } from '@storybook/angular';
import { Breadcrumb, BreadcrumbItem } from './breadcrumb';

const meta: Meta<Breadcrumb> = {
  title: 'Components/Breadcrumb',
  component: Breadcrumb,
  tags: ['autodocs'],
  argTypes: {
    items: {
      description: 'Array of breadcrumb items',
    },
  },
};

export default meta;
type Story = StoryObj<Breadcrumb>;

const sampleItems: BreadcrumbItem[] = [
  { label: 'Home', url: '/' },
  { label: 'Projects', url: '/projects' },
  { label: 'Solution Composer', url: '/projects/solution-composer' },
  { label: 'Configuration' },
];

export const Default: Story = {
  args: {
    items: sampleItems,
  },
  render: (args) => ({
    props: args,
    template: '<ep-breadcrumb [items]="items"></ep-breadcrumb>',
  }),
};

export const Short: Story = {
  args: {
    items: [
      { label: 'Home', url: '/' },
      { label: 'Current Page' },
    ],
  },
  render: (args) => ({
    props: args,
    template: '<ep-breadcrumb [items]="items"></ep-breadcrumb>',
  }),
};

export const Long: Story = {
  args: {
    items: [
      { label: 'Home', url: '/' },
      { label: 'Projects', url: '/projects' },
      { label: 'Engineering', url: '/projects/engineering' },
      { label: 'Solution Composer', url: '/projects/engineering/solution-composer' },
      { label: 'Repository Configuration', url: '/projects/engineering/solution-composer/config' },
      { label: 'Edit' },
    ],
  },
  render: (args) => ({
    props: args,
    template: '<ep-breadcrumb [items]="items"></ep-breadcrumb>',
  }),
};
