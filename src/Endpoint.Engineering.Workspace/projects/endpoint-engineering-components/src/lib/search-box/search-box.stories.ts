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
  },
};

export default meta;
type Story = StoryObj<SearchBox>;

export const Default: Story = {
  args: {
    placeholder: 'Search...',
  },
};

export const SearchConfigurations: Story = {
  args: {
    placeholder: 'Search configurations...',
  },
};

export const WithValue: Story = {
  render: () => ({
    template: `
      <ee-search-box placeholder="Search..." [value]="'e-commerce'"></ee-search-box>
    `,
  }),
};
