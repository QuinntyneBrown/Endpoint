import { Component, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import {
  Button,
  FormSection,
  WizardSteps,
  WizardStep,
  RepositoryCard,
  RepositoryData,
  FolderMappingList,
  FolderMapping,
  JsonPreview
} from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { OutputType, RepositoryConfiguration, FolderConfiguration } from '../../models/alacarte-request.model';

interface WizardRepository extends RepositoryData {
  folders: FolderMapping[];
}

@Component({
  selector: 'app-request-create',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatRadioModule,
    Button,
    FormSection,
    WizardSteps,
    RepositoryCard,
    FolderMappingList,
    JsonPreview
  ],
  templateUrl: './request-create.html',
  styleUrl: './request-create.scss'
})
export class RequestCreatePage {
  // Wizard state
  protected readonly currentStep = signal(0);
  protected readonly steps: WizardStep[] = [
    { label: 'Basic Info' },
    { label: 'Repositories' },
    { label: 'Folders' },
    { label: 'Review' }
  ];

  // Step 1: Basic Info
  protected readonly name = signal('');
  protected readonly solutionName = signal('');
  protected readonly outputDirectory = signal('');
  protected readonly outputType = signal<OutputType>(OutputType.DotNetSolution);

  protected readonly outputTypes = Object.values(OutputType);
  protected readonly OutputType = OutputType;

  // Step 2: Repositories
  protected readonly repositories = signal<WizardRepository[]>([]);

  // Computed values
  protected readonly totalFolders = computed(() =>
    this.repositories().reduce((sum, repo) => sum + repo.folders.length, 0)
  );

  protected readonly requestPreview = computed(() => ({
    name: this.name(),
    solutionName: this.solutionName(),
    outputDirectory: this.outputDirectory(),
    outputType: this.outputType(),
    repositories: this.repositories().map(repo => ({
      url: repo.isLocal ? '' : repo.url,
      localDirectory: repo.isLocal ? repo.localDirectory : undefined,
      branch: repo.branch,
      isLocal: repo.isLocal,
      folders: repo.folders.map(f => ({
        sourcePath: f.sourcePath,
        destinationPath: f.destinationPath
      }))
    }))
  }));

  constructor(
    private requestService: ALaCarteRequestService,
    private router: Router
  ) {}

  // Navigation
  nextStep(): void {
    if (this.currentStep() < this.steps.length - 1) {
      this.currentStep.update(step => step + 1);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 0) {
      this.currentStep.update(step => step - 1);
    }
  }

  goToStep(step: number): void {
    if (step >= 0 && step < this.steps.length) {
      this.currentStep.set(step);
    }
  }

  // Validation
  isStepValid(step: number): boolean {
    switch (step) {
      case 0:
        return !!(this.name() && this.solutionName() && this.outputDirectory());
      case 1:
        return this.repositories().length > 0;
      case 2:
        return this.repositories().every(repo => repo.folders.length > 0);
      case 3:
        return this.isStepValid(0) && this.isStepValid(1) && this.isStepValid(2);
      default:
        return false;
    }
  }

  canProceed(): boolean {
    return this.isStepValid(this.currentStep());
  }

  // Repository management
  addRepository(isLocal: boolean = false): void {
    const newRepo: WizardRepository = {
      id: crypto.randomUUID(),
      url: '',
      branch: 'main',
      isLocal: isLocal,
      localDirectory: '',
      folders: []
    };
    this.repositories.update(repos => [...repos, newRepo]);
  }

  updateRepository(index: number, updates: Partial<WizardRepository>): void {
    this.repositories.update(repos =>
      repos.map((repo, i) => i === index ? { ...repo, ...updates } : repo)
    );
  }

  deleteRepository(repo: RepositoryData): void {
    this.repositories.update(repos => repos.filter(r => r.id !== repo.id));
  }

  onRepositoryUrlChange(index: number, url: string): void {
    this.updateRepository(index, { url });
  }

  onRepositoryBranchChange(index: number, branch: string): void {
    this.updateRepository(index, { branch });
  }

  onRepositoryLocalDirectoryChange(index: number, localDirectory: string): void {
    this.updateRepository(index, { localDirectory });
  }

  // Folder management
  addFolderMapping(repoIndex: number): void {
    const newFolder: FolderMapping = {
      id: crypto.randomUUID(),
      sourcePath: '',
      destinationPath: ''
    };
    this.repositories.update(repos =>
      repos.map((repo, i) =>
        i === repoIndex
          ? { ...repo, folders: [...repo.folders, newFolder] }
          : repo
      )
    );
  }

  updateFolders(repoIndex: number, folders: FolderMapping[]): void {
    this.repositories.update(repos =>
      repos.map((repo, i) =>
        i === repoIndex ? { ...repo, folders } : repo
      )
    );
  }

  deleteFolder(repoIndex: number, folder: FolderMapping): void {
    this.repositories.update(repos =>
      repos.map((repo, i) =>
        i === repoIndex
          ? { ...repo, folders: repo.folders.filter(f => f.id !== folder.id) }
          : repo
      )
    );
  }

  // Actions
  onCancel(): void {
    this.router.navigate(['/requests']);
  }

  onSaveDraft(): void {
    // TODO: Implement draft saving
    console.log('Saving draft...');
  }

  onSubmit(): void {
    if (!this.isStepValid(3)) {
      return;
    }

    const repositories: RepositoryConfiguration[] = this.repositories().map(repo => ({
      url: repo.isLocal ? '' : repo.url,
      branch: repo.branch,
      isLocal: repo.isLocal,
      folders: repo.folders.map(f => ({
        sourcePath: f.sourcePath,
        destinationPath: f.destinationPath
      }))
    }));

    const request = {
      name: this.name(),
      solutionName: this.solutionName(),
      outputDirectory: this.outputDirectory(),
      outputType: this.outputType(),
      repositories
    };

    this.requestService.create(request);
    this.router.navigate(['/requests']);
  }

  getRepoDisplayName(repo: WizardRepository): string {
    if (repo.isLocal && repo.localDirectory) {
      return repo.localDirectory;
    }
    if (repo.url) {
      const parts = repo.url.replace(/\.git$/, '').split('/');
      return parts.slice(-2).join('/');
    }
    return 'New Repository';
  }
}
