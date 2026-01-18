# Endpoint Engineering - Design v2

## VS Code-Inspired UI Mocks with Angular Material Dark Theme

This folder contains mobile-first UI mock designs for the Endpoint Engineering Angular application. The designs clone the VS Code interface using Angular Material components and the standard Indigo/Pink dark theme.

> 📊 **Implementation Status:** See [IMPLEMENTATION_GAP_ANALYSIS.md](./IMPLEMENTATION_GAP_ANALYSIS.md) for a comprehensive audit of what has been implemented vs. the design specifications.

---

## Mock Files

| File | Description |
|------|-------------|
| `00-style-guide.html` | Comprehensive style guide with colors, spacing, typography, and responsive breakpoints |
| `01-global-header.html` | Global header component (VS Code title bar style) |
| `02-rail.html` | Rail component (VS Code Activity Bar style) |
| `03-alacarte-wizard.html` | ALaCarteRequest wizard with multi-step flow |
| `04-full-layout.html` | Complete layout showing all components together |

---

## How to View

1. Open any `.html` file in a modern browser (Chrome, Firefox, Edge, Safari)
2. Use browser DevTools (F12) to toggle device emulation for mobile/tablet views
3. The responsive indicator in the bottom-right shows current breakpoint

---

## Taking Screenshots

### Automated Script (Recommended)

Use the included `generate-screenshots.js` script to automatically generate all PNG files:

```bash
# Install dependencies (first time only)
npm install

# Generate all screenshots
cd designs
node generate-screenshots.js
```

This will generate both desktop (1440x900) and mobile (375x812) screenshots for all HTML files:
- `00-style-guide-desktop.png` / `00-style-guide-mobile.png`
- `01-global-header-desktop.png` / `01-global-header-mobile.png`
- `02-rail-desktop.png` / `02-rail-mobile.png`
- `03-alacarte-wizard-desktop.png` / `03-alacarte-wizard-mobile.png`
- `04-full-layout-desktop.png` / `04-full-layout-mobile.png`

### Using Browser DevTools (Manual)

1. Open the HTML file in Chrome
2. Press `F12` to open DevTools
3. Click the device toggle icon (or press `Ctrl+Shift+M`)
4. Select device or set custom viewport:
   - **Mobile**: 375 x 812 (iPhone X)
   - **Desktop**: 1440 x 900
5. Press `Ctrl+Shift+P` and type "screenshot"
6. Select "Capture screenshot" or "Capture full size screenshot"

---

## Style Guide Summary

### Color Palette (Angular Material Dark Theme - Indigo/Pink)

| Token | Value | Usage |
|-------|-------|-------|
| `--mat-primary` | `#9fa8da` | Primary actions, active states |
| `--mat-accent` | `#f48fb1` | Accent elements, badges |
| `--mat-warn` | `#ef5350` | Errors, destructive actions |
| `--mat-success` | `#81c784` | Success states |
| `--mat-background` | `#0d1117` | Page background |
| `--mat-surface` | `#161b22` | Cards, panels |
| `--mat-surface-elevated` | `#21262d` | Elevated surfaces |
| `--mat-on-surface` | `#e6edf3` | Primary text |
| `--mat-on-surface-medium` | `rgba(230,237,243,0.7)` | Secondary text |
| `--mat-on-surface-disabled` | `rgba(230,237,243,0.38)` | Disabled text |

### Spacing Scale (8px base)

| Token | Value |
|-------|-------|
| `--space-1` | 4px |
| `--space-2` | 8px |
| `--space-3` | 12px |
| `--space-4` | 16px (base) |
| `--space-6` | 24px |
| `--space-8` | 32px |
| `--space-12` | 48px |
| `--space-16` | 64px |

### Responsive Breakpoints

| Breakpoint | Min Width | Description |
|------------|-----------|-------------|
| XS | 0 | Mobile phones (portrait) |
| SM | 576px | Mobile phones (landscape) |
| MD | 768px | Tablets (portrait) - Rail becomes visible |
| LG | 992px | Tablets (landscape) / Small laptops - Sidebar becomes visible |
| XL | 1200px | Desktops - All elements visible |
| XXL | 1400px | Large desktops |

### Component Sizes

| Component | Size |
|-----------|------|
| Header Height | 48px |
| Rail Width | 48px |
| Sidebar Width | 240px |
| Status Bar Height | 22px |
| Button Height (default) | 40px |

---

## Reusable Components

### 1. Global Header

VS Code-inspired title bar with:
- Logo and app name (left)
- Menu items: File, Edit, View, Go, Run, Help (desktop only)
- Search bar with keyboard shortcut indicator (center)
- Action buttons: Notifications, Account (right)
- Window controls: Minimize, Maximize, Close (desktop only)

**Responsive Behavior:**
- Mobile: Hamburger menu, compact logo
- Tablet: Full menu visible
- Desktop: All elements including window controls

### 2. Rail (Activity Bar)

Vertical icon navigation:
- Fixed 48px width on left side
- Icon buttons for main sections
- Active indicator bar (primary color)
- Badge support for notifications
- Hover tooltips
- Settings gear anchored to bottom

**Responsive Behavior:**
- Mobile: Hidden, accessible via hamburger overlay
- Tablet+: Always visible

### 3. ALaCarteRequest Wizard

Multi-step form wizard:
- Step 1: Basic Info (Solution Name, Directory, Output Type)
- Step 2: Repositories (Add Git/Local repositories)
- Step 3: Folders (Configure folder mappings)
- Step 4: Review (Summary + JSON preview)

**Features:**
- Progress stepper with completed/active/pending states
- Repository cards with Git/Local icons
- Folder list with drag handles
- Add buttons with dashed borders
- Back/Next navigation footer

---

## ALaCarteRequest Data Model

```typescript
interface ALaCarteRequest {
  aLaCarteRequestId: string;      // GUID
  repositories: RepositoryConfiguration[];
  outputType: OutputType;         // NotSpecified | DotNetSolution | MixDotNetSolutionWithOtherFolders
  directory: string;              // Output directory path
  solutionName: string;           // e.g., "ALaCarte.sln"
}

interface RepositoryConfiguration {
  repositoryConfigurationId: string;  // GUID
  url: string;                        // Git repository URL
  branch: string;                     // Default: "main"
  localDirectory?: string;            // Alternative to url
  folders: FolderConfiguration[];
}

interface FolderConfiguration {
  from: string;   // Source path in repository
  to: string;     // Destination path
  root?: string;  // Optional root for Angular projects
}
```

---

## Angular Material Components Used

| Component | Angular Material Equivalent |
|-----------|---------------------------|
| Buttons | `mat-button`, `mat-raised-button`, `mat-icon-button` |
| Icons | `mat-icon` with Material Icons font |
| Inputs | `mat-form-field`, `mat-input` |
| Select | `mat-select` |
| Radio | `mat-radio-group`, `mat-radio-button` |
| Cards | `mat-card` |
| Badges | `mat-badge` |
| Tooltips | `mat-tooltip` |
| Stepper | `mat-stepper` (horizontal) |
| Sidenav | `mat-sidenav-container` |
| Toolbar | `mat-toolbar` |

---

## Mobile-First Design Principles

1. **Stack vertically on mobile** - Forms and content flow top-to-bottom
2. **Touch-friendly targets** - Minimum 48x48px tap areas
3. **Progressive disclosure** - Hide non-essential elements on small screens
4. **Thumb-friendly navigation** - Rail icons and footer actions within reach
5. **Readable typography** - 14px base font, adequate line height
6. **Responsive spacing** - Increase padding/margins on larger screens

---

## File Structure for Implementation

```
projects/
  endpoint-engineering/
    src/
      app/
        shell/
          global-header/
            global-header.component.ts
            global-header.component.html
            global-header.component.scss
          rail/
            rail.component.ts
            rail.component.html
            rail.component.scss
          main-layout/
            main-layout.component.ts
            main-layout.component.html
            main-layout.component.scss
        pages/
          alacarte-wizard/
            alacarte-wizard.component.ts
            alacarte-wizard.component.html
            alacarte-wizard.component.scss
            steps/
              basic-info-step/
              repositories-step/
              folders-step/
              review-step/
```
