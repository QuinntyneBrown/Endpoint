import type { Meta, StoryObj } from '@storybook/angular';
import { LogOutput, LogEntry } from './log-output';

const meta: Meta<LogOutput> = {
  title: 'Components/LogOutput',
  component: LogOutput,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Section title',
    },
    maxHeight: {
      control: 'text',
      description: 'Maximum height of log container',
    },
    autoScroll: {
      control: 'boolean',
      description: 'Auto scroll to bottom on new entries',
    },
  },
};

export default meta;
type Story = StoryObj<LogOutput>;

export const ExecutionLog: Story = {
  args: {
    title: 'Execution Log',
    maxHeight: '300px',
    logs: [
      { level: 'info', message: "Starting execution for 'Enterprise API Platform'" },
      { level: 'info', message: 'Output directory: /home/user/output/enterprise' },
      { level: 'success', message: 'Cloning dotnet/aspnetcore from main branch' },
      { level: 'success', message: 'Cloning dotnet/runtime from release/8.0 branch' },
      { level: 'success', message: 'Cloning EntityFramework/EntityFramework from main branch' },
      { level: 'success', message: 'Local directory validated: custom-middleware' },
      { level: 'info', message: 'Validating configured folders...' },
      { level: 'success', message: 'Validated: src/Mvc/Mvc.Core' },
      { level: 'success', message: 'Validated: src/Http/Http.Abstractions' },
      { level: 'info', message: 'Copying project folders to output directory...' },
      { level: 'success', message: 'Copied src/Mvc/Mvc.Core (124 files)' },
    ] as LogEntry[],
  },
};

export const WithErrors: Story = {
  args: {
    title: 'Build Log',
    logs: [
      { level: 'info', message: 'Starting build process...' },
      { level: 'success', message: 'Dependencies restored' },
      { level: 'warning', message: 'Package version mismatch detected' },
      { level: 'error', message: 'Build failed: Missing reference to System.Net.Http' },
      { level: 'error', message: 'Error CS0246: The type or namespace name could not be found' },
    ] as LogEntry[],
  },
};

export const EmptyLog: Story = {
  args: {
    title: 'Activity Log',
    logs: [] as LogEntry[],
  },
};

export const NoTitle: Story = {
  args: {
    logs: [
      { level: 'info', message: 'Processing...' },
      { level: 'success', message: 'Done!' },
    ] as LogEntry[],
  },
};
