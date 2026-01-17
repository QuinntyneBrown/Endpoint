# Endpoint Dashboard Designs

This folder contains 5 complete frontend dashboard design concepts for Endpoint. All designs use Angular Material dark theme colors and are mobile-first responsive.

## Design Overview

### Design 1: Command Center
A terminal-inspired dashboard with command categories and an integrated command terminal.

**Features:**
- Terminal-style command input and output display
- Quick action cards for common operations
- Sidebar navigation with categorized commands
- Mobile bottom navigation
- Command history with status indicators

**Screenshots:**
- Mobile: `design-1-command-center-mobile.png`
- Desktop: `design-1-command-center-desktop.png`

---

### Design 2: Visual Builder
A card-based dashboard focused on visual code generation with builder-centric UI.

**Features:**
- Search bar with voice input option
- Quick start template carousel
- Large builder cards with feature tags
- Activity timeline with status indicators
- Material Design 3 inspired navigation

**Screenshots:**
- Mobile: `design-2-visual-builder-mobile.png`
- Desktop: `design-2-visual-builder-desktop.png`

---

### Design 3: Project Navigator
A file-tree centered design mimicking IDE-style navigation with inline code generation.

**Features:**
- VS Code-inspired file tree navigation
- Expandable/collapsible folder structure
- File status badges (new, modified)
- Context menu for code generation actions
- Quick actions panel for common operations
- Status indicators for .NET SDK and project info

**Screenshots:**
- Mobile: `design-3-project-navigator-mobile.png`
- Desktop: `design-3-project-navigator-desktop.png`

---

### Design 4: Workflow Pipeline
A flow-based generation dashboard showing code generation as visual pipelines.

**Features:**
- Visual pipeline stages with status indicators
- Active workflow progress tracking
- Step-by-step execution visualization
- Workflow templates with stats
- Recent workflows list with filtering

**Screenshots:**
- Mobile: `design-4-workflow-pipeline-mobile.png`
- Desktop: `design-4-workflow-pipeline-desktop.png`

---

### Design 5: Modular Grid
A widget-based customizable dashboard with modular components.

**Features:**
- Overview stats grid (solutions, files, commands, services)
- Quick commands widget with icon buttons
- Recent activity timeline
- Microservices browser widget
- Code output preview widget
- Weekly generation chart widget
- Floating action button (FAB)

**Screenshots:**
- Mobile: `design-5-modular-grid-mobile.png`
- Desktop: `design-5-modular-grid-desktop.png`

---

## Technical Details

### Angular Theme Colors Used
All designs strictly use Angular Material dark theme color palettes:

- **Primary colors**: Deep Purple, Indigo, Cyan, Blue Grey
- **Accent colors**: Teal, Pink, Amber, Orange
- **Background**: #121212, #0d1117, #0a0e14, #0f0f0f
- **Surface levels**: Elevation-based surface colors
- **Text**: High emphasis (87%), Medium (60%), Disabled (38%)

### Mobile-First Approach
- All designs start with mobile viewport (375px)
- Progressive enhancement for tablet (768px) and desktop (1200px+)
- Bottom navigation on mobile, sidebar on desktop
- Touch-friendly tap targets (minimum 48px)

### Typography
- Font: Roboto (system font for Angular)
- Monospace: Roboto Mono / JetBrains Mono (code snippets)
- Material Icons for iconography

### Files
- `design-[1-5]-*.html` - HTML mockups with embedded CSS
- `design-[1-5]-*-mobile.png` - Mobile viewport screenshots
- `design-[1-5]-*-desktop.png` - Desktop viewport screenshots
