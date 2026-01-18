import type { Meta, StoryObj } from '@storybook/angular';
import { ConfigCard } from './config-card';

const meta: Meta<ConfigCard> = {
  title: 'Components/ConfigCard',
  component: ConfigCard,
  tags: ['autodocs'],
  argTypes: {
    name: {
      control: 'text',
      description: 'Configuration name',
    },
    description: {
      control: 'text',
      description: 'Configuration description',
    },
    icon: {
      control: 'text',
      description: 'Material icon name',
    },
    lastUsed: {
      control: 'text',
      description: 'Last used timestamp',
    },
  },
};

export default meta;
type Story = StoryObj<ConfigCard>;

export const ECommerceAPI: Story = {
  args: {
    name: 'E-Commerce API',
    description:
      'Complete e-commerce solution with authentication, product catalog, shopping cart, and order management',
    icon: 'layers',
    lastUsed: 'Used 2 hours ago',
    meta: [
      { icon: 'storage', label: '3 repositories' },
      { icon: 'folder', label: '12 components' },
      { icon: 'schedule', label: 'Updated 2 days ago' },
    ],
    tags: [
      { label: 'GitHub', variant: 'github' },
      { label: 'Local', variant: 'local' },
      { label: '.NET 8' },
    ],
  },
};

export const MicroservicesTemplate: Story = {
  args: {
    name: 'Microservices Template',
    description:
      'Multi-service architecture with API gateway, message bus, and shared libraries',
    icon: 'layers',
    meta: [
      { icon: 'storage', label: '5 repositories' },
      { icon: 'folder', label: '24 components' },
      { icon: 'schedule', label: 'Updated 5 days ago' },
    ],
    tags: [
      { label: 'GitHub', variant: 'github' },
      { label: '.NET 8' },
      { label: 'Docker' },
    ],
  },
};

export const MinimalCard: Story = {
  args: {
    name: 'Minimal API Starter',
    description: 'Lightweight API template with essential features',
  },
};

export const CardWithCustomIcon: Story = {
  args: {
    name: 'Authentication Module',
    description: 'JWT-based authentication with user management',
    icon: 'lock',
    tags: [
      { label: 'Local', variant: 'local' },
      { label: '.NET 8' },
    ],
  },
};
