import type { Meta, StoryObj } from '@storybook/angular';
import { Dialog } from './dialog';

const meta: Meta<Dialog> = {
  title: 'Components/Dialog',
  component: Dialog,
  tags: ['autodocs'],
  argTypes: {
    open: {
      control: 'boolean',
      description: 'Dialog visibility state',
    },
    title: {
      control: 'text',
      description: 'Dialog title',
    },
    width: {
      control: 'select',
      options: ['small', 'medium', 'large'],
      description: 'Dialog width',
    },
  },
};

export default meta;
type Story = StoryObj<Dialog>;

export const Default: Story = {
  args: {
    open: true,
    title: 'Dialog Title',
    width: 'medium',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-dialog [open]="open" [title]="title" [width]="width">
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">
          This is the dialog content. You can put any content here.
        </p>
        <div dialog-actions>
          <button style="background: transparent; color: #e6edf3; padding: 12px 24px; border: 1px solid rgba(230, 237, 243, 0.12); border-radius: 4px; font-weight: 500; cursor: pointer;">
            Cancel
          </button>
          <button style="background: #9fa8da; color: #000; padding: 12px 24px; border: none; border-radius: 4px; font-weight: 500; cursor: pointer;">
            Confirm
          </button>
        </div>
      </ep-dialog>
    `,
  }),
};

export const Small: Story = {
  args: {
    open: true,
    title: 'Confirm Action',
    width: 'small',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-dialog [open]="open" [title]="title" [width]="width">
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">
          Are you sure you want to proceed?
        </p>
        <div dialog-actions>
          <button style="background: transparent; color: #e6edf3; padding: 12px 24px; border: 1px solid rgba(230, 237, 243, 0.12); border-radius: 4px; font-weight: 500; cursor: pointer;">
            No
          </button>
          <button style="background: #9fa8da; color: #000; padding: 12px 24px; border: none; border-radius: 4px; font-weight: 500; cursor: pointer;">
            Yes
          </button>
        </div>
      </ep-dialog>
    `,
  }),
};

export const Large: Story = {
  args: {
    open: true,
    title: 'Configuration Details',
    width: 'large',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-dialog [open]="open" [title]="title" [width]="width">
        <div style="color: rgba(230, 237, 243, 0.7);">
          <h3 style="margin: 0 0 12px 0; color: #e6edf3;">Repository Information</h3>
          <p style="margin: 0 0 16px 0;">Configure your repository settings and folder mappings here. This dialog provides ample space for complex forms and content.</p>
          <h3 style="margin: 0 0 12px 0; color: #e6edf3;">Additional Settings</h3>
          <p style="margin: 0;">You can add multiple sections and detailed configurations in this larger dialog format.</p>
        </div>
        <div dialog-actions>
          <button style="background: transparent; color: #e6edf3; padding: 12px 24px; border: 1px solid rgba(230, 237, 243, 0.12); border-radius: 4px; font-weight: 500; cursor: pointer;">
            Cancel
          </button>
          <button style="background: #9fa8da; color: #000; padding: 12px 24px; border: none; border-radius: 4px; font-weight: 500; cursor: pointer;">
            Save
          </button>
        </div>
      </ep-dialog>
    `,
  }),
};

export const DeleteConfirmation: Story = {
  args: {
    open: true,
    title: 'Delete Configuration',
    width: 'small',
  },
  render: (args) => ({
    props: args,
    template: `
      <ep-dialog [open]="open" [title]="title" [width]="width">
        <p style="margin: 0; color: rgba(230, 237, 243, 0.7);">
          This action cannot be undone. Are you sure you want to delete this configuration?
        </p>
        <div dialog-actions>
          <button style="background: transparent; color: #e6edf3; padding: 12px 24px; border: 1px solid rgba(230, 237, 243, 0.12); border-radius: 4px; font-weight: 500; cursor: pointer;">
            Cancel
          </button>
          <button style="background: #ef5350; color: #fff; padding: 12px 24px; border: none; border-radius: 4px; font-weight: 500; cursor: pointer;">
            Delete
          </button>
        </div>
      </ep-dialog>
    `,
  }),
};
