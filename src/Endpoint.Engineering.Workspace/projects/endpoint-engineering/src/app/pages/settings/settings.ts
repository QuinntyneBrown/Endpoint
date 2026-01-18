import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Button, FormSection, PageHeader } from 'endpoint-engineering-components';

@Component({
  selector: 'app-settings',
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    Button,
    FormSection,
    PageHeader
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class SettingsPage {
  // General settings
  protected readonly defaultOutputDirectory = signal('/projects/output');
  protected readonly defaultBranch = signal('main');
  protected readonly autoSaveDrafts = signal(true);

  // Appearance settings
  protected readonly theme = signal('dark');
  protected readonly showWelcomeOnStartup = signal(true);

  // Git settings
  protected readonly gitUsername = signal('');
  protected readonly gitEmail = signal('');

  onSave(): void {
    // TODO: Implement settings persistence
    console.log('Settings saved');
  }

  onReset(): void {
    this.defaultOutputDirectory.set('/projects/output');
    this.defaultBranch.set('main');
    this.autoSaveDrafts.set(true);
    this.theme.set('dark');
    this.showWelcomeOnStartup.set(true);
    this.gitUsername.set('');
    this.gitEmail.set('');
  }
}
