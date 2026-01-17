import type { Meta, StoryObj } from '@storybook/angular';
import { WizardSteps, WizardStep } from './wizard-steps';

const meta: Meta<WizardSteps> = {
  title: 'Components/WizardSteps',
  component: WizardSteps,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<WizardSteps>;

const steps: WizardStep[] = [
  { label: 'Basic Info' },
  { label: 'Select Components' },
  { label: 'Configure' },
  { label: 'Review' },
];

export const FirstStep: Story = {
  args: {
    steps: steps,
    currentStep: 0,
  },
};

export const MiddleStep: Story = {
  args: {
    steps: steps,
    currentStep: 2,
  },
};

export const LastStep: Story = {
  args: {
    steps: steps,
    currentStep: 3,
  },
};

export const ThreeSteps: Story = {
  args: {
    steps: [
      { label: 'Start' },
      { label: 'Process' },
      { label: 'Complete' },
    ],
    currentStep: 1,
  },
};
