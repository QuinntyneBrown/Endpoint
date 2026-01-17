# Endpoint Engineering Components

A comprehensive Angular component library for the Solution Composer application, built following the coding guidelines and Material Design 3 principles.

## Overview

This library provides reusable UI components with consistent theming, spacing, and interactions based on the detailed designs in `designs/detailed-designs-solution-composer`.

## Design System

### Theme
- **Primary Color**: Indigo (#9fa8da)
- **Accent Color**: Pink (#f48fb1)
- **Background**: Dark (#0d1117)
- **Surface**: Elevated dark (#161b22, #21262d)

### Spacing Scale
- XS: 4px
- SM: 8px
- MD: 12px
- LG: 16px
- XL: 24px
- XXL: 32px
- XXXL: 48px

## Components

### Foundation Components

#### Button (`ep-button`)
Multi-variant button component supporting primary, secondary, danger, and icon styles.

**Props:**
- `variant`: 'primary' | 'secondary' | 'danger' | 'icon'
- `size`: 'small' | 'medium' | 'large'
- `disabled`: boolean
- `fullWidth`: boolean
- `type`: 'button' | 'submit' | 'reset'

**Usage:**
```html
<ep-button variant="primary" size="medium">Click Me</ep-button>
<ep-button variant="icon"><span class="material-icons">settings</span></ep-button>
```

#### Card (`ep-card`)
Container component with multiple visual variants and configurable padding.

**Props:**
- `variant`: 'default' | 'elevated' | 'outlined'
- `padding`: 'none' | 'small' | 'medium' | 'large'
- `hoverable`: boolean

**Usage:**
```html
<ep-card variant="elevated" padding="medium" [hoverable]="true">
  <h3>Card Title</h3>
  <p>Card content goes here</p>
</ep-card>
```

#### Badge (`ep-badge`)
Status indicator component with color variants and size options.

**Props:**
- `variant`: 'primary' | 'success' | 'warning' | 'danger' | 'neutral'
- `size`: 'small' | 'medium'
- `dot`: boolean

**Usage:**
```html
<ep-badge variant="success">Active</ep-badge>
<ep-badge variant="primary" [dot]="true"></ep-badge>
```

### Navigation Components

#### AppHeader (`ep-app-header`)
Sticky header component with back button and action slots.

**Props:**
- `title`: string
- `showBackButton`: boolean

**Events:**
- `backClick`: void

**Usage:**
```html
<ep-app-header title="Solution Composer" [showBackButton]="true" (backClick)="goBack()">
  <button>Action</button>
</ep-app-header>
```

#### Breadcrumb (`ep-breadcrumb`)
Navigation breadcrumb component with clickable items.

**Props:**
- `items`: BreadcrumbItem[]

**Events:**
- `itemClick`: BreadcrumbItem

**Usage:**
```typescript
items: BreadcrumbItem[] = [
  { label: 'Home', url: '/' },
  { label: 'Projects', url: '/projects' },
  { label: 'Current' }
];
```
```html
<ep-breadcrumb [items]="items" (itemClick)="navigate($event)"></ep-breadcrumb>
```

### Data Display Components

#### TreeView (`ep-tree-view`)
Hierarchical tree view for file/folder navigation with expand/collapse.

**Props:**
- `nodes`: TreeNode[]
- `selectable`: boolean
- `expandable`: boolean

**Events:**
- `nodeClick`: TreeNode
- `nodeToggle`: TreeNode

**Usage:**
```typescript
nodes: TreeNode[] = [
  {
    id: '1',
    label: 'src',
    icon: 'folder',
    expanded: true,
    children: [
      { id: '1-1', label: 'app.component.ts', icon: 'code' }
    ]
  }
];
```
```html
<ep-tree-view [nodes]="nodes" [selectable]="true" (nodeClick)="onSelect($event)"></ep-tree-view>
```

### Form Components

#### FormInput (`ep-form-input`)
Text input component with label, validation, and helper text.

**Props:**
- `label`: string
- `placeholder`: string
- `type`: 'text' | 'email' | 'password' | 'url' | 'number'
- `required`: boolean
- `disabled`: boolean
- `helperText`: string
- `errorText`: string

**Usage:**
```html
<ep-form-input 
  label="Email" 
  type="email" 
  [required]="true"
  helperText="Enter your email address"
  errorText="Invalid email">
</ep-form-input>
```

### Interactive Components

#### Dialog (`ep-dialog`)
Modal dialog component with configurable size and action slots.

**Props:**
- `open`: boolean
- `title`: string
- `width`: 'small' | 'medium' | 'large'

**Events:**
- `close`: void

**Usage:**
```html
<ep-dialog [open]="isOpen" title="Confirm" width="small" (close)="onClose()">
  <p>Are you sure?</p>
  <div dialog-actions>
    <ep-button variant="secondary">Cancel</ep-button>
    <ep-button variant="primary">Confirm</ep-button>
  </div>
</ep-dialog>
```

#### SearchBox (`ep-search-box`)
Search input component with clear functionality.

**Props:**
- `placeholder`: string
- `value`: string

**Events:**
- `valueChange`: string
- `search`: string

**Usage:**
```html
<ep-search-box 
  placeholder="Search repositories..." 
  [(value)]="searchQuery"
  (search)="onSearch($event)">
</ep-search-box>
```

#### EmptyState (`ep-empty-state`)
Empty state component with icon, title, and action slots.

**Props:**
- `icon`: string (Material icon name)
- `title`: string
- `description`: string

**Usage:**
```html
<ep-empty-state 
  icon="inbox" 
  title="No items" 
  description="Get started by creating your first item">
  <ep-button variant="primary">Create Item</ep-button>
</ep-empty-state>
```

## Development

### Building the Library
```bash
ng build endpoint-engineering-components
```

### Running Storybook
```bash
npx storybook dev -p 6006 --config-dir projects/endpoint-engineering-components/.storybook
```

### Coding Guidelines
This library follows the coding guidelines in `docs/coding-guidelines.md`:
- Component naming without "Component" suffix
- Separate files for HTML, SCSS, and TypeScript
- BEM naming for CSS classes (prefixed with `ep-`)
- Design tokens for consistent spacing
- Material Design 3 theming
- Barrel exports (index.ts) for each component

## Design Tokens

All components use centralized design tokens from `src/lib/tokens/design-tokens.scss`:
- Color tokens
- Spacing scale
- Typography
- Border radius
- Transitions
- Shadows
- Z-index scale

Import in component SCSS:
```scss
@import '../../tokens/design-tokens.scss';

.my-component {
  padding: var(--spacing-md);
  background: var(--mat-surface);
  color: var(--mat-on-surface);
}
```

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
