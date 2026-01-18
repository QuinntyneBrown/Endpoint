import type { Meta, StoryObj } from '@storybook/angular';
import { WizardSteps, WizardStep } from './wizard-steps';

const meta: Meta<WizardSteps> = {
  title: 'Components/WizardSteps',
  component: WizardSteps,
  tags: ['autodocs'],
  argTypes: {
    currentStep: {
      control: { type: 'number', min: 0 },
      description: 'Index of the current active step',
    },
  },
};

export default meta;
type Story = StoryObj<WizardSteps>;

const defaultSteps: WizardStep[] = [
  { label: 'Basic Info' },
  { label: 'Select Components' },
  { label: 'Configure' },
];

export const StepOne: Story = {
  args: {
    steps: defaultSteps,
    currentStep: 0,
  },
};

export const StepTwo: Story = {
  args: {
    steps: defaultSteps,
    currentStep: 1,
  },
};

export const StepThree: Story = {
  args: {
    steps: defaultSteps,
    currentStep: 2,
  },
};

export const AllCompleted: Story = {
  args: {
    steps: [
      { label: 'Basic Info', completed: true },
      { label: 'Select Components', completed: true },
      { label: 'Configure', completed: true },
    ],
    currentStep: 3,
  },
};

export const FiveSteps: Story = {
  args: {
    steps: [
      { label: 'Start' },
      { label: 'Details' },
      { label: 'Components' },
      { label: 'Review' },
      { label: 'Complete' },
    ],
    currentStep: 2,
  },
};
