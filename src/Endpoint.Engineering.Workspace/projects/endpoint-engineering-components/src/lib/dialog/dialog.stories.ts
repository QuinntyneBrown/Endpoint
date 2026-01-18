import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Dialog } from './dialog';
import { Button } from '../button';

const meta: Meta<Dialog> = {
  title: 'Components/Dialog',
  component: Dialog,
  tags: ['autodocs'],
  decorators: [
    moduleMetadata({
      imports: [Button],
    }),
  ],
  argTypes: {
    title: {
      control: 'text',
      description: 'Dialog title',
    },
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    showCloseButton: {
      control: 'boolean',
      description: 'Show close button',
    },
    open: {
      control: 'boolean',
      description: 'Whether dialog is open',
    },
  },
};

export default meta;
type Story = StoryObj<Dialog>;

export const SaveConfiguration: Story = {
  args: {
    title: 'Save Repository Configuration',
    icon: 'save',
    showCloseButton: true,
    open: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-dialog [title]="title" [icon]="icon" [showCloseButton]="showCloseButton" [open]="open">
        <div style="color: #e6edf3;">
          <p style="margin-bottom: 16px;">Enter a name for this configuration.</p>
          <input type="text" placeholder="Configuration name" style="width: 100%; padding: 12px; background: #161b22; border: 1px solid rgba(230,237,243,0.12); border-radius: 8px; color: #e6edf3;" />
        </div>
        <div actions>
          <ee-button variant="secondary">Cancel</ee-button>
          <ee-button icon="save">Save Configuration</ee-button>
        </div>
      </ee-dialog>
    `,
  }),
};

export const ConfirmDelete: Story = {
  args: {
    title: 'Delete Request?',
    icon: 'warning',
    showCloseButton: false,
    open: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-dialog [title]="title" [icon]="icon" [showCloseButton]="showCloseButton" [open]="open">
        <div style="color: #e6edf3;">
          <p>Are you sure you want to delete this request? This action cannot be undone.</p>
        </div>
        <div actions>
          <ee-button variant="secondary">Cancel</ee-button>
          <ee-button variant="danger" icon="delete_forever">Delete</ee-button>
        </div>
      </ee-dialog>
    `,
  }),
};

export const SimpleDialog: Story = {
  args: {
    title: 'Information',
    open: true,
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-dialog [title]="title" [open]="open">
        <div style="color: #e6edf3;">
          <p>This is a simple dialog with just content.</p>
        </div>
        <div actions>
          <ee-button>OK</ee-button>
        </div>
      </ee-dialog>
    `,
  }),
};
