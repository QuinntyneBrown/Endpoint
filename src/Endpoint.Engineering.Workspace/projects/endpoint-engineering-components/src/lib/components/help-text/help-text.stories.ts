import type { Meta, StoryObj } from '@storybook/angular';
import { HelpText } from './help-text';

const meta: Meta<HelpText> = {
  title: 'Components/HelpText',
  component: HelpText,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<HelpText>;

export const Info: Story = {
  args: {
    variant: 'info',
    title: 'Information',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-help-text [variant]="variant" [title]="title">
        This is helpful information to guide the user through the process.
      </ep-help-text>
    `,
  }),
};

export const Warning: Story = {
  args: {
    variant: 'warning',
    title: 'Important',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-help-text [variant]="variant" [title]="title">
        Please read this carefully before proceeding with the action.
      </ep-help-text>
    `,
  }),
};

export const Tip: Story = {
  args: {
    variant: 'tip',
    title: 'Pro Tip',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-help-text [variant]="variant" [title]="title">
        Use keyboard shortcuts to speed up your workflow. Press Ctrl+S to save.
      </ep-help-text>
    `,
  }),
};

export const WithoutTitle: Story = {
  args: {
    variant: 'info',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-help-text [variant]="variant">
        This is a help text without a title, just the body content.
      </ep-help-text>
    `,
  }),
};

export const CustomIcon: Story = {
  args: {
    variant: 'info',
    title: 'Custom Icon',
    icon: 'help',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-help-text [variant]="variant" [title]="title" [icon]="icon">
        You can customize the icon by providing a different Material icon name.
      </ep-help-text>
    `,
  }),
};
