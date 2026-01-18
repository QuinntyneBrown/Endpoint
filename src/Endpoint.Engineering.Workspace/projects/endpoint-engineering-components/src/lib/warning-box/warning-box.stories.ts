import type { Meta, StoryObj } from '@storybook/angular';
import { WarningBox } from './warning-box';

const meta: Meta<WarningBox> = {
  title: 'Components/WarningBox',
  component: WarningBox,
  tags: ['autodocs'],
  argTypes: {
    message: {
      control: 'text',
      description: 'Warning message text',
    },
    title: {
      control: 'text',
      description: 'Optional title',
    },
    variant: {
      control: 'select',
      options: ['warning', 'error', 'info', 'success'],
      description: 'Warning box variant',
    },
  },
};

export default meta;
type Story = StoryObj<WarningBox>;

export const Warning: Story = {
  args: {
    variant: 'warning',
    title: 'Warning:',
    message: 'This action may take a few minutes to complete.',
  },
};

export const Error: Story = {
  args: {
    variant: 'error',
    title: 'Error:',
    message: 'Deleting this request is permanent. All configuration data will be lost. Generated solutions will not be affected.',
  },
};

export const Info: Story = {
  args: {
    variant: 'info',
    message: 'You can customize the output directory in the settings.',
  },
};

export const Success: Story = {
  args: {
    variant: 'success',
    title: 'Success!',
    message: 'Your configuration has been saved successfully.',
  },
};

export const NoTitle: Story = {
  args: {
    variant: 'warning',
    message: 'Please review the changes before proceeding.',
  },
};

export const AllVariants: Story = {
  render: () => ({
    template: `
      <div style="display: flex; flex-direction: column; gap: 16px;">
        <ee-warning-box variant="info" title="Info:" message="This is an informational message."></ee-warning-box>
        <ee-warning-box variant="success" title="Success:" message="Operation completed successfully."></ee-warning-box>
        <ee-warning-box variant="warning" title="Warning:" message="Please be careful with this action."></ee-warning-box>
        <ee-warning-box variant="error" title="Error:" message="Something went wrong."></ee-warning-box>
      </div>
    `,
  }),
};
