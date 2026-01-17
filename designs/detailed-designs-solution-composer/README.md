# Detailed Designs - Solution Composer

This folder contains detailed design mockups for the Solution Composer (A La Carte) feature. These designs show various states, interactions, and workflows for composing custom solutions from repository components.

## Design Overview

### 01: Initial Empty Layout
**Purpose:** Starting point for developers implementing the Solution Composer.

**Features:**
- Empty state with clear call-to-action
- "New Composition" and "Load Saved Configuration" buttons
- Quick start options (GitHub, Local, Recent, Templates)
- Clean, welcoming interface for first-time users

**Screenshots:**
- Mobile: `01-initial-empty-layout-mobile.png`
- Desktop: `01-initial-empty-layout-desktop.png`

---

### 02: Composing A La Carte Input
**Purpose:** User input wizard for creating a new composition.

**Features:**
- Multi-step wizard (Basic Info, Select Components, Configure)
- Solution name and output directory inputs
- Target framework selection (.NET 8, .NET 7, .NET 6)
- Generation options (Unit Tests, Documentation, Docker, CI/CD)
- Form validation and helper text
- Progress indicator

**Screenshots:**
- Mobile: `02-composing-alacarte-input-mobile.png`
- Desktop: `02-composing-alacarte-input-desktop.png`

---

### 03: Repository Configuration Management
**Purpose:** Adding, removing, and reordering repository configurations.

**Features:**
- Drag-and-drop reordering with handles
- Repository cards with type badges (GitHub/Local)
- Nested folder management within each repository
- Visual order indicators (numbered)
- Add/Edit/Delete actions
- Collapsible folder sections
- Help text explaining drag functionality

**Screenshots:**
- Mobile: `03-repository-config-management-mobile.png`
- Desktop: `03-repository-config-management-desktop.png`

---

### 04: Saved Configurations List
**Purpose:** Browse and manage persisted repository configurations.

**Features:**
- Grid layout of configuration cards
- Search and filter functionality
- Configuration metadata (repo count, component count, last updated)
- Usage statistics bar (total configs, repos, components)
- Tags for quick identification (GitHub, Local, .NET version)
- "Last used" badges for recently accessed configs
- Quick actions menu on each card

**Screenshots:**
- Mobile: `04-saved-configurations-list-mobile.png`
- Desktop: `04-saved-configurations-list-desktop.png`

---

### 05: Save Repository Configuration
**Purpose:** Dialog for saving a repository configuration.

**Features:**
- Modal dialog overlay
- Configuration name input
- Description textarea
- Tag/label management
- Save location dropdown
- Preview summary (repositories, folders, components count)
- Save and Cancel actions

**Screenshots:**
- Mobile: `05-save-repository-configuration-mobile.png`
- Desktop: `05-save-repository-configuration-desktop.png`

---

### 06: Folder Configuration Drag Order
**Purpose:** Managing folder order within a single repository.

**Features:**
- Drag handles for reordering folders
- Numbered order indicators
- Folder path display with breadcrumb
- Statistics summary (total folders, total files)
- Add folder button
- Visual feedback for drag operations
- Folder type icons

**Screenshots:**
- Mobile: `06-folder-configuration-drag-order-mobile.png`
- Desktop: `06-folder-configuration-drag-order-desktop.png`

---

### 07: Select Folder from Filesystem
**Purpose:** Browse and select folders from local file system.

**Features:**
- Tree view of local directories
- Breadcrumb navigation
- Expand/collapse folder structure
- Checkbox selection for multiple folders
- Current path display
- Home directory quick access
- Fixed selection bar showing selected count
- Browse and Select actions

**Screenshots:**
- Mobile: `07-select-folder-filesystem-mobile.png`
- Desktop: `07-select-folder-filesystem-desktop.png`

---

### 08: Git Repository Folder Navigator
**Purpose:** Browse and select folders from Git repositories (GitHub/GitLab/BitBucket).

**Features:**
- Git URL input with validation
- Branch/tag selector dropdown
- Repository info card with stats (stars, forks, last commit)
- Tree view of repository structure
- Folder selection with checkboxes
- Clone/fetch status indicator
- Support for multiple Git platforms
- Repository metadata display

**Screenshots:**
- Mobile: `08-git-repo-folder-navigator-mobile.png`
- Desktop: `08-git-repo-folder-navigator-desktop.png`

---

### 09: A La Carte Output Preview
**Purpose:** View the results after running the A La Carte command.

**Features:**
- Success banner with generation time
- Statistics cards (files generated, total size, time taken)
- Tree view of generated folder structure
- File preview section with syntax highlighting
- Generation log/console output
- Action buttons (Open in Editor, Open Folder, Run Build)
- Error/warning indicators if applicable
- Expandable/collapsible sections

**Screenshots:**
- Mobile: `09-alacarte-output-preview-mobile.png`
- Desktop: `09-alacarte-output-preview-desktop.png`

---

## Technical Details

### Design System
All designs use consistent Angular Material dark theme:

- **Primary**: Indigo (#9fa8da)
- **Accent**: Pink (#f48fb1)
- **Background**: Dark (#0d1117)
- **Surface**: Elevated dark (#161b22, #21262d)
- **Typography**: Roboto font family
- **Icons**: Material Icons

### Responsive Design
- **Mobile**: 375px width (iPhone viewport)
- **Desktop**: 1440px width (standard laptop)
- Mobile-first approach with progressive enhancement
- Touch-friendly targets (minimum 48px)
- Adaptive layouts for different screen sizes

### Interaction Patterns
- Drag-and-drop for reordering
- Expandable/collapsible sections
- Modal dialogs for complex forms
- Breadcrumb navigation
- Tree views for hierarchical data
- Search and filter capabilities
- Real-time validation feedback

### Files
- HTML mockups with embedded CSS
- Full-page screenshots in PNG format
- Both mobile and desktop versions for each design

## Usage

To regenerate screenshots after modifying HTML files:

```bash
cd designs
node render-detailed-designs.js
```

This will use Playwright to render all designs at mobile (375x812) and desktop (1440x900) viewports.
