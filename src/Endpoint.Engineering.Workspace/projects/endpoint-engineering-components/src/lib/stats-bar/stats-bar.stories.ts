import type { Meta, StoryObj } from '@storybook/angular';
import { StatsBar, StatItem } from './stats-bar';

const meta: Meta<StatsBar> = {
  title: 'Components/StatsBar',
  component: StatsBar,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<StatsBar>;

export const ConfigurationStats: Story = {
  args: {
    stats: [
      { value: 12, label: 'Saved Configs' },
      { value: 48, label: 'Total Repos' },
      { value: 127, label: 'Components' },
    ] as StatItem[],
  },
};

export const ProjectStats: Story = {
  args: {
    stats: [
      { value: 5, label: 'Projects' },
      { value: 23, label: 'Branches' },
      { value: 156, label: 'Commits' },
    ] as StatItem[],
  },
};

export const TwoStats: Story = {
  args: {
    stats: [
      { value: 3, label: 'Repositories' },
      { value: 12, label: 'Folders' },
    ] as StatItem[],
  },
};

export const WithFormattedValues: Story = {
  args: {
    stats: [
      { value: '1.2K', label: 'Downloads' },
      { value: '99%', label: 'Uptime' },
      { value: '4.8', label: 'Rating' },
    ] as StatItem[],
  },
};
