import type { Meta, StoryObj } from '@storybook/angular';
import { EndpointEngineeringComponents } from './endpoint-engineering-components';

const meta: Meta<EndpointEngineeringComponents> = {
  title: 'Components/EndpointEngineeringComponents',
  component: EndpointEngineeringComponents,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<EndpointEngineeringComponents>;

export const Default: Story = {};
