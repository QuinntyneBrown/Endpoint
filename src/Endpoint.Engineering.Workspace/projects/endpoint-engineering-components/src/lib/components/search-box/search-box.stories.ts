import type { Meta, StoryObj } from '@storybook/angular';
import { SearchBox } from './search-box';

const meta: Meta<SearchBox> = {
  title: 'Components/SearchBox',
  component: SearchBox,
  tags: ['autodocs'],
  argTypes: {
    placeholder: {
      control: 'text',
      description: 'Placeholder text',
    },
    value: {
      control: 'text',
      description: 'Search input value',
    },
  },
};

export default meta;
type Story = StoryObj<SearchBox>;

export const Default: Story = {
  args: {
    placeholder: 'Search...',
    value: '',
  },
  render: (args) => ({
    props: args,
    template: '<ep-search-box [placeholder]="placeholder" [value]="value"></ep-search-box>',
  }),
};

export const WithValue: Story = {
  args: {
    placeholder: 'Search configurations...',
    value: 'angular',
  },
  render: (args) => ({
    props: args,
    template: '<ep-search-box [placeholder]="placeholder" [value]="value"></ep-search-box>',
  }),
};

export const CustomPlaceholder: Story = {
  args: {
    placeholder: 'Search for repositories, folders, or files...',
    value: '',
  },
  render: (args) => ({
    props: args,
    template: '<ep-search-box [placeholder]="placeholder" [value]="value"></ep-search-box>',
  }),
};
