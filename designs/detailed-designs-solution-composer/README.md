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

---

## ALaCarteRequest CRUD Operations

The following designs cover the complete lifecycle of managing ALaCarteRequest objects - the core data model that stores repository configurations for reuse.

### 10: Create ALaCarteRequest
**Purpose:** Form to create and save a new ALaCarteRequest for later use.

**Features:**
- Request name/title input for identification
- Solution name configuration
- Output directory picker with browse button
- OutputType dropdown (DotNetSolution, AngularWorkspace, etc.)
- Repository management section
- Add/edit/delete repositories with URL, branch, folder configurations
- Repository preview cards showing folder count
- Save and Cancel actions

**Screenshots:**
- Mobile: `10-create-alacarte-request-mobile.png`
- Desktop: `10-create-alacarte-request-desktop.png`

---

### 11: List ALaCarteRequests
**Purpose:** Browse and manage all saved ALaCarteRequest configurations.

**Features:**
- Statistics bar showing total requests, executions, weekly usage
- Search box with real-time filtering
- Filter chips (All, Recent, Favorites)
- Sortable table with columns: Name, Solution Name, Repositories, Created, Last Used
- Action buttons per row: View, Edit, Execute, Delete
- Pagination controls
- "New Request" button in header
- Empty state when no requests exist

**Screenshots:**
- Mobile: `11-list-alacarte-requests-mobile.png`
- Desktop: `11-list-alacarte-requests-desktop.png`

---

### 12: View ALaCarteRequest
**Purpose:** Read-only detail view of a saved ALaCarteRequest.

**Features:**
- Request metadata (name, created date, last used, execution count)
- Configuration summary (solution name, output directory, output type badge)
- Complete list of all configured repositories
- Each repository shows: URL/local path, branch, configured folders
- Folder details with source and destination paths
- Action buttons: Execute, Edit, Clone Request, Delete
- Breadcrumb navigation back to list

**Screenshots:**
- Mobile: `12-view-alacarte-request-mobile.png`
- Desktop: `12-view-alacarte-request-desktop.png`

---

### 13: Edit ALaCarteRequest
**Purpose:** Modify an existing ALaCarteRequest configuration.

**Features:**
- Pre-filled form with existing values
- "Unsaved changes" indicator in header
- Modified fields highlighted with accent color
- All editable fields: name, solution name, output directory, output type
- Add/remove/reorder repositories with drag handles
- Modify repository configurations (URL, branch, folders)
- Discard Changes confirmation if data modified
- Save Changes and Cancel buttons

**Screenshots:**
- Mobile: `13-edit-alacarte-request-mobile.png`
- Desktop: `13-edit-alacarte-request-desktop.png`

---

### 14: Delete ALaCarteRequest
**Purpose:** Confirmation dialog for deleting an ALaCarteRequest.

**Features:**
- Modal overlay with warning design
- Request name and key information displayed
- Impact summary showing what will be deleted:
  - Number of repositories
  - Number of folder configurations
  - Associated metadata and logs
- Confirmation checkbox required to enable delete
- Warning about permanent deletion
- Cancel and Delete (danger) buttons

**Screenshots:**
- Mobile: `14-delete-alacarte-request-mobile.png`
- Desktop: `14-delete-alacarte-request-desktop.png`

---

### 15: Execute ALaCarteRequest
**Purpose:** Execute a saved ALaCarteRequest and monitor progress in real-time.

**Features:**
- Configuration summary at top (request name, output directory)
- Overall progress bar with percentage
- Stage-by-stage progress indicators:
  - Initialize (Completed)
  - Clone Repositories (Completed)
  - Copy Folders (Processing)
  - Create Solution (Pending)
  - Finalize (Pending)
- Repository status cards showing individual processing
- Real-time log output section with color-coded messages (info, success, warning, error)
- Auto-scroll to latest log entries
- Cancel Execution button
- Success state showing completion time and actions

**Screenshots:**
- Mobile: `15-execute-alacarte-request-mobile.png`
- Desktop: `15-execute-alacarte-request-desktop.png`

---

### 16: Inspect Solution Output
**Purpose:** Browse and preview the generated solution files in the UI.

**Features:**
- Breadcrumb navigation showing current path in solution
- Statistics bar: Total Files, Total Size, Projects, Generation Time
- Split-pane layout (resizable):
  - Left: File/folder tree view with expand/collapse
  - Right: File preview pane
- Tree view with appropriate icons (folder, C#, JSON, config files)
- Code preview with syntax highlighting
- File metadata display (size, type, last modified)
- Action buttons: Open in Editor, Copy Path, Open Folder, Refresh
- Search within file tree
- Expandable/collapsible sections

**Screenshots:**
- Mobile: `16-inspect-solution-output-mobile.png`
- Desktop: `16-inspect-solution-output-desktop.png`

---
