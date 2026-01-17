import type { Meta, StoryObj } from '@storybook/angular';
import { FormInput } from './form-input';

const meta: Meta<FormInput> = {
  title: 'Components/FormInput',
  component: FormInput,
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Input label',
    },
    placeholder: {
      control: 'text',
      description: 'Placeholder text',
    },
    type: {
      control: 'select',
      options: ['text', 'email', 'password', 'url', 'number'],
      description: 'Input type',
    },
    required: {
      control: 'boolean',
      description: 'Required field',
    },
    disabled: {
      control: 'boolean',
      description: 'Disabled state',
    },
    helperText: {
      control: 'text',
      description: 'Helper text below input',
    },
    errorText: {
      control: 'text',
      description: 'Error message',
    },
  },
};

export default meta;
type Story = StoryObj<FormInput>;

export const Default: Story = {
  args: {
    label: 'Email',
    placeholder: 'Enter your email',
    type: 'email',
    required: false,
    disabled: false,
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [required]="required" [disabled]="disabled"></ep-form-input>',
  }),
};

export const Required: Story = {
  args: {
    label: 'Username',
    placeholder: 'Enter username',
    type: 'text',
    required: true,
    helperText: 'This field is required',
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [required]="required" [helperText]="helperText"></ep-form-input>',
  }),
};

export const WithHelper: Story = {
  args: {
    label: 'Repository URL',
    placeholder: 'https://github.com/user/repo',
    type: 'url',
    helperText: 'Enter the full URL to your Git repository',
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [helperText]="helperText"></ep-form-input>',
  }),
};

export const WithError: Story = {
  args: {
    label: 'Email',
    placeholder: 'Enter your email',
    type: 'email',
    errorText: 'Please enter a valid email address',
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [errorText]="errorText"></ep-form-input>',
  }),
};

export const Disabled: Story = {
  args: {
    label: 'Disabled Field',
    placeholder: 'Cannot edit',
    type: 'text',
    disabled: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [disabled]="disabled"></ep-form-input>',
  }),
};

export const Password: Story = {
  args: {
    label: 'Password',
    placeholder: 'Enter password',
    type: 'password',
    required: true,
  },
  render: (args) => ({
    props: args,
    template: '<ep-form-input [label]="label" [placeholder]="placeholder" [type]="type" [required]="required"></ep-form-input>',
  }),
};
