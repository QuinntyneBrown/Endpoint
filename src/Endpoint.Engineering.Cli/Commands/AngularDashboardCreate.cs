// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("angular-dashboard-create", HelpText = "Creates an Angular 21 workspace with a Material Design dark theme dashboard")]
public class AngularDashboardCreateRequest : IRequest
{
    [Option('n', "name", Required = false, Default = "admin-workspace",
        HelpText = "The name of the Angular workspace")]
    public string WorkspaceName { get; set; } = "admin-workspace";

    [Option('p', "project", Required = false,
        HelpText = "The name of the main application project (defaults to workspace name without '-workspace' suffix)")]
    public string? ProjectName { get; set; }

    [Option('d', "directory", Required = false,
        HelpText = "Output directory for the workspace (defaults to current directory)")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('l', "library", Required = false, Default = false,
        HelpText = "Create a separate component library project")]
    public bool CreateLibrary { get; set; }

    [Option("lib-name", Required = false,
        HelpText = "Name of the component library (defaults to project name with '-components' suffix)")]
    public string? LibraryName { get; set; }

    [Option("prefix", Required = false, Default = "app",
        HelpText = "Selector prefix for components (e.g., 'app' for app-dashboard)")]
    public string Prefix { get; set; } = "app";

    [Option("skip-install", Required = false, Default = false,
        HelpText = "Skip npm install after creating the workspace")]
    public bool SkipInstall { get; set; }

    [Option("open", Required = false, Default = true,
        HelpText = "Open the workspace in VS Code after creation")]
    public bool OpenInVsCode { get; set; }
}

public class AngularDashboardCreateRequestHandler : IRequestHandler<AngularDashboardCreateRequest>
{
    private readonly ILogger<AngularDashboardCreateRequestHandler> _logger;
    private readonly ICommandService _commandService;
    private readonly IFileSystem _fileSystem;

    public AngularDashboardCreateRequestHandler(
        ILogger<AngularDashboardCreateRequestHandler> logger,
        ICommandService commandService,
        IFileSystem fileSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(AngularDashboardCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Angular dashboard workspace: {WorkspaceName}", request.WorkspaceName);

        var workspacePath = _fileSystem.Path.Combine(request.Directory, request.WorkspaceName);
        var projectName = request.ProjectName ?? request.WorkspaceName.Replace("-workspace", string.Empty);
        var libraryName = request.LibraryName ?? $"{projectName}-components";

        // Step 1: Create Angular workspace
        _logger.LogInformation("Creating Angular workspace with ng new");
        _commandService.Start(
            $"ng new {request.WorkspaceName} --no-create-application --skip-install --defaults=true",
            request.Directory);

        // Step 2: Create main application project
        _logger.LogInformation("Creating main application project: {ProjectName}", projectName);
        _commandService.Start(
            $"ng generate application {projectName} --prefix={request.Prefix} --style=scss --routing=true --skip-tests=false",
            workspacePath);

        // Step 3: Create component library if requested
        if (request.CreateLibrary)
        {
            _logger.LogInformation("Creating component library: {LibraryName}", libraryName);
            _commandService.Start(
                $"ng generate library {libraryName} --prefix={request.Prefix}",
                workspacePath);
        }

        // Step 4: Add Angular Material
        _logger.LogInformation("Adding Angular Material");
        _commandService.Start(
            $"ng add @angular/material --project={projectName} --theme=custom --animations=enabled --typography=true --skip-confirmation",
            workspacePath);

        // Step 4b: Install zone.js (required for Angular change detection)
        _logger.LogInformation("Installing zone.js");
        _commandService.Start("npm install zone.js --save", workspacePath);

        // Step 5: Create project files
        var projectPath = _fileSystem.Path.Combine(workspacePath, "projects", projectName, "src");
        var appPath = _fileSystem.Path.Combine(projectPath, "app");

        // Create directory structure
        CreateDirectoryStructure(appPath);

        // Write application files
        await WriteAppFilesAsync(appPath, projectName, request.Prefix);

        // Write main.ts with zone.js import
        await WriteMainTsAsync(projectPath);

        // Write styles
        await WriteStylesAsync(projectPath);

        // Write dashboard page
        await WriteDashboardPageAsync(appPath, request.Prefix);

        // Write shell components
        await WriteShellComponentsAsync(appPath, request.Prefix);

        // Write barrel exports for parent folders
        await WriteBarrelExportsAsync(appPath);

        // Step 6: Create component library files if requested
        if (request.CreateLibrary)
        {
            var libPath = _fileSystem.Path.Combine(workspacePath, "projects", libraryName, "src", "lib");
            await WriteLibraryFilesAsync(libPath, libraryName, request.Prefix);
        }

        // Step 7: Install dependencies
        if (!request.SkipInstall)
        {
            _logger.LogInformation("Installing npm dependencies");
            _commandService.Start("npm install", workspacePath);
        }

        // Step 8: Open in VS Code
        if (request.OpenInVsCode)
        {
            _logger.LogInformation("Opening workspace in VS Code");
            _commandService.Start("code .", workspacePath);
        }

        _logger.LogInformation("Angular dashboard workspace created successfully at: {Path}", workspacePath);
    }

    private void CreateDirectoryStructure(string appPath)
    {
        var directories = new[]
        {
            _fileSystem.Path.Combine(appPath, "pages", "dashboard"),
            _fileSystem.Path.Combine(appPath, "shell", "main-layout"),
            _fileSystem.Path.Combine(appPath, "shell", "global-header"),
            _fileSystem.Path.Combine(appPath, "shell", "sidenav"),
            _fileSystem.Path.Combine(appPath, "components", "hello-world-tile"),
            _fileSystem.Path.Combine(appPath, "services"),
            _fileSystem.Path.Combine(appPath, "models")
        };

        foreach (var dir in directories)
        {
            _fileSystem.Directory.CreateDirectory(dir);
        }
    }

    private async Task WriteAppFilesAsync(string appPath, string projectName, string prefix)
    {
        // app.ts
        var appTs = $@"import {{ Component }} from '@angular/core';
import {{ MainLayout }} from './shell/main-layout/main-layout';

@Component({{
  selector: '{prefix}-root',
  imports: [MainLayout],
  templateUrl: './app.html',
  styleUrl: './app.scss'
}})
export class App {{
  protected readonly title = '{projectName}';
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "app.ts"), appTs);

        // app.html
        var appHtml = @"<app-main-layout></app-main-layout>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "app.html"), appHtml);

        // app.scss
        var appScss = @":host {
  display: block;
  height: 100%;
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "app.scss"), appScss);

        // app.routes.ts
        var appRoutes = @"import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.Dashboard)
  }
];
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "app.routes.ts"), appRoutes);

        // app.config.ts
        var appConfig = @"import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient()
  ]
};
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "app.config.ts"), appConfig);
    }

    private async Task WriteMainTsAsync(string projectPath)
    {
        var mainTs = @"import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(projectPath, "main.ts"), mainTs);
    }

    private async Task WriteStylesAsync(string projectPath)
    {
        var styles = @"@use '@angular/material' as mat;

@include mat.core();

/* Material Design 3 Dark Theme - Blue/Orange Color Palette */
/* Based on ADMIN-UI-IMPLEMENTATION-GUIDE.md */
:root {
  // ============================================
  // PRIMARY COLORS (Blue)
  // ============================================
  --primary-50: #e3f2fd;
  --primary-100: #bbdefb;
  --primary-200: #90caf9;
  --primary-300: #64b5f6;
  --primary-400: #42a5f5;
  --primary-500: #2196f3;   // Main brand color
  --primary-600: #1e88e5;   // Hover state
  --primary-700: #1976d2;   // Active/pressed state
  --primary-800: #1565c0;
  --primary-900: #0d47a1;

  // ============================================
  // ACCENT COLORS (Orange)
  // ============================================
  --accent-500: #ff9800;    // Secondary actions, FAB
  --accent-600: #fb8c00;
  --accent-700: #f57c00;

  // ============================================
  // WARN/ERROR COLORS (Red)
  // ============================================
  --warn-500: #f44336;      // Destructive actions
  --warn-600: #e53935;
  --warn-700: #d32f2f;

  // ============================================
  // STATUS COLORS
  // ============================================
  --status-success: #4caf50;    // Green - active, success
  --status-warning: #ff9800;    // Orange - warning, locked
  --status-error: #f44336;      // Red - error, inactive
  --status-info: #2196f3;       // Blue - information

  // ============================================
  // SURFACE COLORS (Dark Theme)
  // ============================================
  --surface-background: #121212;           // Page background
  --surface-card: #1e1e1e;                 // Cards, toolbar, sidenav
  --surface-elevated: #2d2d2d;             // Elevated elements within cards
  --surface-dialog: #2d2d2d;               // Modal backgrounds
  --surface-divider: rgba(255, 255, 255, 0.12);
  --surface-hover: rgba(255, 255, 255, 0.08);
  --surface-selected: rgba(33, 150, 243, 0.16);

  // ============================================
  // TEXT COLORS
  // ============================================
  --text-primary: rgba(255, 255, 255, 0.87);
  --text-secondary: rgba(255, 255, 255, 0.60);
  --text-disabled: rgba(255, 255, 255, 0.38);
  --text-hint: rgba(255, 255, 255, 0.38);

  // ============================================
  // SPACING (8px base unit)
  // ============================================
  --spacing-xs: 4px;     // 0.5x
  --spacing-sm: 8px;     // 1x
  --spacing-md: 16px;    // 2x
  --spacing-lg: 24px;    // 3x
  --spacing-xl: 32px;    // 4x
  --spacing-xxl: 48px;   // 6x

  // ============================================
  // BORDER RADIUS
  // ============================================
  --radius-xs: 2px;
  --radius-sm: 4px;      // Buttons, inputs
  --radius-md: 8px;      // Cards, dialogs
  --radius-lg: 12px;     // Large containers
  --radius-chip: 16px;   // Chips, pills
  --radius-full: 50%;    // Circular elements

  // ============================================
  // ELEVATION (Box Shadows)
  // ============================================
  --elevation-0: none;
  --elevation-1: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24);
  --elevation-2: 0 3px 6px rgba(0, 0, 0, 0.15), 0 2px 4px rgba(0, 0, 0, 0.12);
  --elevation-4: 0 10px 20px rgba(0, 0, 0, 0.15), 0 3px 6px rgba(0, 0, 0, 0.10);
  --elevation-8: 0 15px 25px rgba(0, 0, 0, 0.15), 0 5px 10px rgba(0, 0, 0, 0.05);
  --elevation-16: 0 20px 40px rgba(0, 0, 0, 0.2);
  --elevation-24: 0 25px 50px rgba(0, 0, 0, 0.25);

  // ============================================
  // TYPOGRAPHY
  // ============================================
  --font-family: 'Roboto', sans-serif;
  --font-family-mono: 'Roboto Mono', monospace;

  --font-size-xs: 11px;
  --font-size-sm: 12px;
  --font-size-md: 13px;
  --font-size-base: 14px;
  --font-size-lg: 16px;
  --font-size-xl: 18px;
  --font-size-xxl: 20px;
  --font-size-title: 24px;
  --font-size-display: 28px;

  // ============================================
  // COMPONENT SIZES
  // ============================================
  --header-height: 64px;
  --sidenav-width: 256px;

  // ============================================
  // TRANSITIONS
  // ============================================
  --transition-fast: 150ms ease;
  --transition-standard: 300ms ease;
  --transition-slow: 500ms ease;
}

/* Global Resets */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body {
  height: 100%;
}

body {
  font-family: var(--font-family);
  background: var(--surface-background);
  color: var(--text-primary);
  font-size: var(--font-size-base);
  line-height: 1.5;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Material Component Overrides */
.mat-mdc-button,
.mat-mdc-outlined-button,
.mat-mdc-raised-button,
.mat-mdc-flat-button {
  --mdc-filled-button-label-text-color: white;
  --mdc-filled-button-container-color: var(--primary-500);
}

.mat-mdc-form-field {
  --mdc-filled-text-field-container-color: var(--surface-elevated);
  --mdc-filled-text-field-label-text-color: var(--text-secondary);
  --mdc-filled-text-field-input-text-color: var(--text-primary);
  --mdc-filled-text-field-focus-label-text-color: var(--primary-500);
  --mdc-filled-text-field-focus-active-indicator-color: var(--primary-500);
}

.mat-mdc-menu-panel {
  background: var(--surface-elevated) !important;

  .mat-mdc-menu-item {
    color: var(--text-primary);

    &:hover {
      background: var(--surface-hover);
    }
  }
}

/* Scrollbar Styling */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: var(--surface-background);
}

::-webkit-scrollbar-thumb {
  background: var(--surface-elevated);
  border-radius: 4px;

  &:hover {
    background: var(--text-disabled);
  }
}

/* Selection */
::selection {
  background: var(--primary-500);
  color: white;
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(projectPath, "styles.scss"), styles);
    }

    private async Task WriteDashboardPageAsync(string appPath, string prefix)
    {
        var dashboardPath = _fileSystem.Path.Combine(appPath, "pages", "dashboard");

        // dashboard.ts
        var dashboardTs = $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ HelloWorldTile }} from '../../components/hello-world-tile/hello-world-tile';

@Component({{
  selector: '{prefix}-dashboard',
  imports: [CommonModule, HelloWorldTile],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
}})
export class Dashboard {{
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(dashboardPath, "dashboard.ts"), dashboardTs);

        // dashboard.html
        var dashboardHtml = @"<div class=""dashboard"">
  <div class=""dashboard__header"">
    <h1 class=""dashboard__title"">Dashboard</h1>
    <p class=""dashboard__subtitle"">Welcome to your admin dashboard</p>
  </div>

  <div class=""dashboard__grid"">
    <app-hello-world-tile></app-hello-world-tile>
  </div>
</div>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(dashboardPath, "dashboard.html"), dashboardHtml);

        // dashboard.scss
        var dashboardScss = @".dashboard {
  padding: var(--spacing-xl);

  &__header {
    margin-bottom: var(--spacing-xl);
  }

  &__title {
    font-size: var(--font-size-display);
    font-weight: 400;
    color: var(--text-primary);
    margin-bottom: var(--spacing-xs);
  }

  &__subtitle {
    font-size: var(--font-size-base);
    color: var(--text-secondary);
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: var(--spacing-lg);
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(dashboardPath, "dashboard.scss"), dashboardScss);

        // index.ts
        var indexTs = @"export * from './dashboard';
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(dashboardPath, "index.ts"), indexTs);

        // Hello World Tile component
        var tilePath = _fileSystem.Path.Combine(appPath, "components", "hello-world-tile");

        // hello-world-tile.ts
        var tileTs = $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ MatCardModule }} from '@angular/material/card';
import {{ MatIconModule }} from '@angular/material/icon';

@Component({{
  selector: '{prefix}-hello-world-tile',
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './hello-world-tile.html',
  styleUrl: './hello-world-tile.scss'
}})
export class HelloWorldTile {{
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "hello-world-tile.ts"), tileTs);

        // hello-world-tile.html
        var tileHtml = @"<div class=""tile"">
  <div class=""tile__icon"">
    <mat-icon>waving_hand</mat-icon>
  </div>
  <div class=""tile__content"">
    <h2 class=""tile__title"">Hello World</h2>
    <p class=""tile__description"">
      Your Angular dashboard is ready! Start building your admin interface
      by adding more tiles and components.
    </p>
  </div>
  <div class=""tile__footer"">
    <span class=""tile__status tile__status--success"">
      <mat-icon>check_circle</mat-icon>
      Ready
    </span>
  </div>
</div>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "hello-world-tile.html"), tileHtml);

        // hello-world-tile.scss
        var tileScss = @".tile {
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  box-shadow: var(--elevation-1);
  transition: box-shadow var(--transition-fast);

  &:hover {
    box-shadow: var(--elevation-2);
  }

  &__icon {
    width: 56px;
    height: 56px;
    border-radius: var(--radius-md);
    background: linear-gradient(135deg, var(--primary-500), var(--accent-500));
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: var(--spacing-md);

    mat-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
      color: white;
    }
  }

  &__content {
    margin-bottom: var(--spacing-md);
  }

  &__title {
    font-size: var(--font-size-xl);
    font-weight: 500;
    color: var(--text-primary);
    margin-bottom: var(--spacing-sm);
  }

  &__description {
    font-size: var(--font-size-base);
    color: var(--text-secondary);
    line-height: 1.6;
  }

  &__footer {
    padding-top: var(--spacing-md);
    border-top: 1px solid var(--surface-divider);
  }

  &__status {
    display: inline-flex;
    align-items: center;
    gap: var(--spacing-xs);
    padding: var(--spacing-xs) var(--spacing-sm);
    border-radius: var(--radius-chip);
    font-size: var(--font-size-sm);
    font-weight: 500;

    mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    &--success {
      background-color: rgba(76, 175, 80, 0.2);
      color: var(--status-success);
    }

    &--warning {
      background-color: rgba(255, 152, 0, 0.2);
      color: var(--status-warning);
    }

    &--error {
      background-color: rgba(244, 67, 54, 0.2);
      color: var(--status-error);
    }
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "hello-world-tile.scss"), tileScss);

        // tile index.ts
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "index.ts"), @"export * from './hello-world-tile';
");
    }

    private async Task WriteShellComponentsAsync(string appPath, string prefix)
    {
        // Main Layout
        var mainLayoutPath = _fileSystem.Path.Combine(appPath, "shell", "main-layout");

        var mainLayoutTs = $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ RouterOutlet }} from '@angular/router';
import {{ GlobalHeader }} from '../global-header/global-header';
import {{ Sidenav }} from '../sidenav/sidenav';

@Component({{
  selector: '{prefix}-main-layout',
  imports: [CommonModule, RouterOutlet, GlobalHeader, Sidenav],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
}})
export class MainLayout {{
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(mainLayoutPath, "main-layout.ts"), mainLayoutTs);

        var mainLayoutHtml = @"<div class=""layout"">
  <app-global-header class=""layout__header""></app-global-header>
  <div class=""layout__container"">
    <app-sidenav class=""layout__sidenav""></app-sidenav>
    <main class=""layout__content"">
      <router-outlet></router-outlet>
    </main>
  </div>
</div>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(mainLayoutPath, "main-layout.html"), mainLayoutHtml);

        var mainLayoutScss = @".layout {
  display: flex;
  flex-direction: column;
  height: 100vh;

  &__header {
    flex-shrink: 0;
    z-index: 1000;
  }

  &__container {
    display: flex;
    flex: 1;
    overflow: hidden;
  }

  &__sidenav {
    flex-shrink: 0;
  }

  &__content {
    flex: 1;
    overflow-y: auto;
    background-color: var(--surface-background);
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(mainLayoutPath, "main-layout.scss"), mainLayoutScss);

        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(mainLayoutPath, "index.ts"), @"export * from './main-layout';
");

        // Global Header
        var headerPath = _fileSystem.Path.Combine(appPath, "shell", "global-header");

        var headerTs = $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ MatToolbarModule }} from '@angular/material/toolbar';
import {{ MatIconModule }} from '@angular/material/icon';
import {{ MatButtonModule }} from '@angular/material/button';
import {{ MatMenuModule }} from '@angular/material/menu';

@Component({{
  selector: '{prefix}-global-header',
  imports: [CommonModule, MatToolbarModule, MatIconModule, MatButtonModule, MatMenuModule],
  templateUrl: './global-header.html',
  styleUrl: './global-header.scss'
}})
export class GlobalHeader {{
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(headerPath, "global-header.ts"), headerTs);

        var headerHtml = @"<mat-toolbar class=""header"">
  <button mat-icon-button class=""header__menu-btn"">
    <mat-icon>menu</mat-icon>
  </button>
  <span class=""header__title"">Admin Dashboard</span>
  <span class=""header__spacer""></span>
  <button mat-icon-button>
    <mat-icon>notifications</mat-icon>
  </button>
  <button mat-icon-button [matMenuTriggerFor]=""userMenu"">
    <mat-icon>account_circle</mat-icon>
  </button>
  <mat-menu #userMenu=""matMenu"">
    <button mat-menu-item>
      <mat-icon>person</mat-icon>
      <span>Profile</span>
    </button>
    <button mat-menu-item>
      <mat-icon>settings</mat-icon>
      <span>Settings</span>
    </button>
    <button mat-menu-item>
      <mat-icon>logout</mat-icon>
      <span>Logout</span>
    </button>
  </mat-menu>
</mat-toolbar>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(headerPath, "global-header.html"), headerHtml);

        var headerScss = @".header {
  background-color: var(--surface-card);
  color: var(--text-primary);
  height: var(--header-height);
  box-shadow: var(--elevation-4);

  &__menu-btn {
    margin-right: var(--spacing-md);
  }

  &__title {
    font-size: var(--font-size-xxl);
    font-weight: 500;
  }

  &__spacer {
    flex: 1;
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(headerPath, "global-header.scss"), headerScss);

        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(headerPath, "index.ts"), @"export * from './global-header';
");

        // Sidenav
        var sidenavPath = _fileSystem.Path.Combine(appPath, "shell", "sidenav");

        var sidenavTs = $@"import {{ Component }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ RouterModule }} from '@angular/router';
import {{ MatIconModule }} from '@angular/material/icon';

interface NavItem {{
  icon: string;
  label: string;
  route: string;
}}

@Component({{
  selector: '{prefix}-sidenav',
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss'
}})
export class Sidenav {{
  navItems: NavItem[] = [
    {{ icon: 'dashboard', label: 'Dashboard', route: '/dashboard' }},
    {{ icon: 'people', label: 'Users', route: '/users' }},
    {{ icon: 'admin_panel_settings', label: 'Roles', route: '/roles' }},
    {{ icon: 'settings', label: 'Settings', route: '/settings' }}
  ];
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(sidenavPath, "sidenav.ts"), sidenavTs);

        var sidenavHtml = @"<nav class=""sidenav"">
  <ul class=""sidenav__list"">
    @for (item of navItems; track item.route) {
      <li class=""sidenav__item"" routerLinkActive=""sidenav__item--active"">
        <a [routerLink]=""item.route"" class=""sidenav__link"">
          <mat-icon class=""sidenav__icon"">{{ item.icon }}</mat-icon>
          <span class=""sidenav__label"">{{ item.label }}</span>
        </a>
      </li>
    }
  </ul>
</nav>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(sidenavPath, "sidenav.html"), sidenavHtml);

        var sidenavScss = @".sidenav {
  width: var(--sidenav-width);
  background-color: var(--surface-card);
  border-right: 1px solid var(--surface-divider);
  height: 100%;
  overflow-y: auto;

  &__list {
    list-style: none;
    padding: var(--spacing-sm) 0;
    margin: 0;
  }

  &__item {
    &--active {
      .sidenav__link {
        background-color: var(--surface-selected);
        color: var(--primary-500);
      }

      .sidenav__icon {
        color: var(--primary-500);
      }
    }
  }

  &__link {
    display: flex;
    align-items: center;
    padding: var(--spacing-md) var(--spacing-lg);
    color: var(--text-primary);
    text-decoration: none;
    transition: background-color var(--transition-fast);

    &:hover {
      background-color: var(--surface-hover);
    }
  }

  &__icon {
    margin-right: var(--spacing-md);
    color: var(--text-secondary);
  }

  &__label {
    font-size: var(--font-size-base);
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(sidenavPath, "sidenav.scss"), sidenavScss);

        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(sidenavPath, "index.ts"), @"export * from './sidenav';
");
    }

    private async Task WriteBarrelExportsAsync(string appPath)
    {
        // shell/index.ts
        var shellIndex = @"export * from './main-layout';
export * from './global-header';
export * from './sidenav';
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "shell", "index.ts"), shellIndex);

        // components/index.ts
        var componentsIndex = @"export * from './hello-world-tile';
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "components", "index.ts"), componentsIndex);

        // pages/index.ts
        var pagesIndex = @"export * from './dashboard';
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(appPath, "pages", "index.ts"), pagesIndex);
    }

    private async Task WriteLibraryFilesAsync(string libPath, string libraryName, string prefix)
    {
        // Create a sample tile component in the library
        var tilePath = _fileSystem.Path.Combine(libPath, "stat-tile");
        _fileSystem.Directory.CreateDirectory(tilePath);

        var tileTs = $@"import {{ Component, Input }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ MatIconModule }} from '@angular/material/icon';

@Component({{
  selector: '{prefix}-stat-tile',
  imports: [CommonModule, MatIconModule],
  templateUrl: './stat-tile.html',
  styleUrl: './stat-tile.scss'
}})
export class StatTile {{
  @Input() title = '';
  @Input() value = '';
  @Input() icon = 'analytics';
  @Input() trend: 'up' | 'down' | 'neutral' = 'neutral';
}}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "stat-tile.ts"), tileTs);

        var tileHtml = @"<div class=""stat-tile"">
  <div class=""stat-tile__icon"">
    <mat-icon>{{ icon }}</mat-icon>
  </div>
  <div class=""stat-tile__content"">
    <span class=""stat-tile__value"">{{ value }}</span>
    <span class=""stat-tile__title"">{{ title }}</span>
  </div>
  @if (trend !== 'neutral') {
    <div class=""stat-tile__trend"" [class.stat-tile__trend--up]=""trend === 'up'"" [class.stat-tile__trend--down]=""trend === 'down'"">
      <mat-icon>{{ trend === 'up' ? 'trending_up' : 'trending_down' }}</mat-icon>
    </div>
  }
</div>
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "stat-tile.html"), tileHtml);

        var tileScss = @".stat-tile {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  background-color: var(--surface-card);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  box-shadow: var(--elevation-1);

  &__icon {
    width: 48px;
    height: 48px;
    border-radius: var(--radius-md);
    background-color: var(--surface-elevated);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--primary-500);
  }

  &__content {
    display: flex;
    flex-direction: column;
    flex: 1;
  }

  &__value {
    font-size: var(--font-size-title);
    font-weight: 500;
    color: var(--text-primary);
  }

  &__title {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
  }

  &__trend {
    width: 32px;
    height: 32px;
    border-radius: var(--radius-full);
    display: flex;
    align-items: center;
    justify-content: center;

    mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    &--up {
      background-color: rgba(76, 175, 80, 0.2);
      color: var(--status-success);
    }

    &--down {
      background-color: rgba(244, 67, 54, 0.2);
      color: var(--status-error);
    }
  }
}
";
        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "stat-tile.scss"), tileScss);

        await _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(tilePath, "index.ts"), @"export * from './stat-tile';
");

        // Update public-api.ts
        var publicApiPath = _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(libPath)!, "public-api.ts");
        var publicApi = @"/*
 * Public API Surface of the component library
 */

export * from './lib/stat-tile';
";
        await _fileSystem.File.WriteAllTextAsync(publicApiPath, publicApi);
    }
}
