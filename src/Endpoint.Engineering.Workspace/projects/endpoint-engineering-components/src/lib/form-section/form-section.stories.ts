import type { Meta, StoryObj } from '@storybook/angular';
import { FormSection } from './form-section';

const meta: Meta<FormSection> = {
  title: 'Components/FormSection',
  component: FormSection,
  tags: ['autodocs'],
  argTypes: {
    title: {
      control: 'text',
      description: 'Section title',
    },
  },
};

export default meta;
type Story = StoryObj<FormSection>;

export const WithTitle: Story = {
  args: {
    title: 'Project Information',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-form-section [title]="title">
        <div style="color: #e6edf3;">
          <p style="margin-bottom: 12px;">Form content goes here</p>
          <input type="text" placeholder="Example input" style="width: 100%; padding: 10px; background: #21262d; border: 1px solid rgba(230,237,243,0.12); border-radius: 6px; color: #e6edf3;" />
        </div>
      </ee-form-section>
    `,
  }),
};

export const WithoutTitle: Story = {
  args: {
    title: '',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-form-section [title]="title">
        <div style="color: #e6edf3;">
          <p>Section without a title</p>
        </div>
      </ee-form-section>
    `,
  }),
};

export const TargetFramework: Story = {
  args: {
    title: 'Target Framework',
  },
  render: (args) => ({
    props: args,
    template: `
      <ee-form-section [title]="title">
        <div style="display: flex; flex-direction: column; gap: 8px;">
          <div style="display: flex; align-items: center; gap: 12px; padding: 12px; background: rgba(159, 168, 218, 0.08); border: 1px solid #9fa8da; border-radius: 8px; color: #e6edf3;">
            <span style="color: #9fa8da;">●</span> .NET 8 (Recommended)
          </div>
          <div style="display: flex; align-items: center; gap: 12px; padding: 12px; background: #21262d; border: 1px solid rgba(230,237,243,0.12); border-radius: 8px; color: #e6edf3;">
            <span style="color: rgba(230,237,243,0.38);">○</span> .NET 7
          </div>
          <div style="display: flex; align-items: center; gap: 12px; padding: 12px; background: #21262d; border: 1px solid rgba(230,237,243,0.12); border-radius: 8px; color: #e6edf3;">
            <span style="color: rgba(230,237,243,0.38);">○</span> .NET 6 (LTS)
          </div>
        </div>
      </ee-form-section>
    `,
  }),
};
