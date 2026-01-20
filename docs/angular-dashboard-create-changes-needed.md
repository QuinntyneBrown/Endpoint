# Angular Dashboard Create - Generator Fixes Needed

This document tracks manual fixes made to the generated Angular dashboard code to comply with `ADMIN-UI-IMPLEMENTATION-GUIDE.md` and `coding-guidelines.md`. These fixes should be incorporated back into the generator.

**Generated:** 2026-01-20
**Generator Command:** `angular-dashboard-create`
**Output Location:** `generated-output/admin-dashboard`

---

## Fixes Required

### 1. Missing `zone.js` Import in `main.ts`

**Issue:** The generated `main.ts` does not import `zone.js`, causing the Angular application to fail at runtime.

**Guideline Reference:** Angular requires zone.js for change detection.

**File:** `projects/{project-name}/src/main.ts`

**Fix Applied:**
```typescript
// BEFORE
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// AFTER
import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
```

**Generator Update Required:**
1. Add `import 'zone.js';` as the first line in the generated `main.ts`
2. Run `npm install zone.js --save` after adding Angular Material

---

### 2. Missing `zone.js` Package Installation

**Issue:** The `zone.js` package is imported but not installed as a dependency, causing the app to fail with "Failed to resolve import zone.js" error.

**Guideline Reference:** Angular requires zone.js for change detection.

**Fix Applied:**
```bash
npm install zone.js --save
```

**Generator Update Required:** Add `RunCommand("npm install zone.js --save", workspacePath);` after the Angular Material installation step.

---

### 3. Missing Barrel Exports (`index.ts`) in Parent Folders

**Issue:** The coding guidelines require barrel exports (`index.ts`) for every folder, but the generator only creates them for leaf component folders.

**Guideline Reference:** `coding-guidelines.md` Section 7.5 - "Create barrel exports (`index.ts`) for every folder, exporting all TypeScript code except test code."

**Files Created:**

**`projects/{project-name}/src/app/shell/index.ts`:**
```typescript
export * from './main-layout';
export * from './global-header';
export * from './sidenav';
```

**`projects/{project-name}/src/app/components/index.ts`:**
```typescript
export * from './hello-world-tile';
```

**`projects/{project-name}/src/app/pages/index.ts`:**
```typescript
export * from './dashboard';
```

**Generator Update Required:** Add `index.ts` barrel export files to `shell/`, `components/`, and `pages/` directories that re-export all child folders.

---

### 4. Hard-coded Active State in Sidenav Instead of `routerLinkActive`

**Issue:** The sidenav uses a hard-coded `active: true` property in the NavItem interface and binds it with `[class.sidenav__item--active]="item.active"`. This doesn't respond to actual route changes.

**Guideline Reference:** Angular best practice - use `routerLinkActive` directive for automatic active state management.

**Files:**
- `projects/{project-name}/src/app/shell/sidenav/sidenav.ts`
- `projects/{project-name}/src/app/shell/sidenav/sidenav.html`

**Fix Applied:**

**sidenav.ts - Remove `active` property from interface:**
```typescript
// BEFORE
interface NavItem {
  icon: string;
  label: string;
  route: string;
  active?: boolean;
}
// navItems includes: { icon: 'dashboard', label: 'Dashboard', route: '/dashboard', active: true }

// AFTER
interface NavItem {
  icon: string;
  label: string;
  route: string;
}
// navItems: { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' }
```

**sidenav.html - Use routerLinkActive directive:**
```html
<!-- BEFORE -->
<li class="sidenav__item" [class.sidenav__item--active]="item.active">

<!-- AFTER -->
<li class="sidenav__item" routerLinkActive="sidenav__item--active">
```

**Generator Update Required:**
1. Remove `active?: boolean` from NavItem interface
2. Remove `active: true` from first nav item
3. Use `routerLinkActive="sidenav__item--active"` instead of `[class.sidenav__item--active]="item.active"`

---

## Summary

| # | Issue | Severity | Status |
|---|-------|----------|--------|
| 1 | Missing zone.js import in main.ts | Critical (app won't run) | Fixed in Generator (CLI + Playground) |
| 2 | Missing zone.js package installation | Critical (app won't run) | Fixed in Generator (CLI + Playground) |
| 3 | Missing barrel exports in parent folders | Medium (coding guidelines) | Fixed in Generator (CLI + Playground) |
| 4 | Hard-coded active state in sidenav | Medium (best practice) | Fixed in Generator (CLI + Playground) |

---

## Generator Files to Update

The following files in the generator need to be updated:

1. **`AngularDashboardCreate.cs`** (or equivalent generator file)
   - `WriteAppFilesAsync()` - Add zone.js import to main.ts content
   - `CreateDirectoryStructure()` - Add creation of parent folder index.ts files
   - `WriteShellComponentsAsync()` - Fix sidenav to use routerLinkActive

