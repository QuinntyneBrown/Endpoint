import type { Meta, StoryObj } from '@storybook/angular';
import { StageList, Stage } from './stage-list';

const meta: Meta<StageList> = {
  title: 'Components/StageList',
  component: StageList,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<StageList>;

export const InProgress: Story = {
  args: {
    stages: [
      { name: 'Clone Repositories', status: 'completed', description: '5 of 5 repositories cloned' },
      { name: 'Validate Folders', status: 'completed', description: '9 folders validated' },
      { name: 'Copy Project Folders', status: 'active', description: 'Copying files to output directory...' },
      { name: 'Create Solution File', status: 'pending', description: 'Waiting...' },
      { name: 'Link Projects', status: 'pending', description: 'Waiting...' },
    ] as Stage[],
  },
};

export const AllCompleted: Story = {
  args: {
    stages: [
      { name: 'Clone Repositories', status: 'completed', description: '5 of 5 repositories cloned' },
      { name: 'Validate Folders', status: 'completed', description: '9 folders validated' },
      { name: 'Copy Project Folders', status: 'completed', description: 'All files copied' },
      { name: 'Create Solution File', status: 'completed', description: 'Solution created' },
      { name: 'Link Projects', status: 'completed', description: 'All projects linked' },
    ] as Stage[],
  },
};

export const WithError: Story = {
  args: {
    stages: [
      { name: 'Clone Repositories', status: 'completed', description: '4 of 5 repositories cloned' },
      { name: 'Validate Folders', status: 'error', description: 'Missing folder: src/Auth' },
      { name: 'Copy Project Folders', status: 'pending', description: 'Waiting...' },
    ] as Stage[],
  },
};

export const JustStarted: Story = {
  args: {
    stages: [
      { name: 'Initialize', status: 'active', description: 'Setting up environment...' },
      { name: 'Download Dependencies', status: 'pending' },
      { name: 'Build Project', status: 'pending' },
      { name: 'Run Tests', status: 'pending' },
    ] as Stage[],
  },
};
