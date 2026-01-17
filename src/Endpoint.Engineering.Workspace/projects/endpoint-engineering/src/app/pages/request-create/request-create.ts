import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppHeader, Button, Card, FormInput, Badge } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { OutputType, RepositoryConfig } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-create',
  imports: [CommonModule, FormsModule, AppHeader, Button, Card, FormInput, Badge],
  templateUrl: './request-create.html',
  styleUrl: './request-create.scss'
})
export class RequestCreate {
  requestName = '';
  solutionName = '';
  outputDirectory = '';
  outputType: OutputType = 'dotnet';
  repositories: RepositoryConfig[] = [
    {
      id: '1',
      type: 'github',
      url: 'https://github.com/dotnet/aspnetcore',
      branch: 'main',
      folders: [
        { id: 'f1', path: 'src/Mvc', order: 0 },
        { id: 'f2', path: 'src/Identity', order: 1 },
        { id: 'f3', path: 'src/Security', order: 2 },
        { id: 'f4', path: 'src/Hosting', order: 3 },
        { id: 'f5', path: 'src/Http', order: 4 }
      ],
      order: 0
    },
    {
      id: '2',
      type: 'github',
      url: 'https://github.com/dotnet/runtime',
      branch: 'release/8.0',
      folders: [
        { id: 'f6', path: 'src/libraries/System.Text.Json', order: 0 },
        { id: 'f7', path: 'src/libraries/System.Linq', order: 1 },
        { id: 'f8', path: 'src/libraries/System.Collections', order: 2 }
      ],
      order: 1
    },
    {
      id: '3',
      type: 'local',
      url: '/home/user/projects/my-custom-lib',
      folders: [
        { id: 'f9', path: 'utilities', order: 0 },
        { id: 'f10', path: 'extensions', order: 1 }
      ],
      order: 2
    }
  ];

  outputTypes: { value: OutputType; label: string }[] = [
    { value: 'dotnet', label: 'DotNet Solution (.sln)' },
    { value: 'angular', label: 'Angular Workspace' },
    { value: 'node', label: 'Node.js Package' },
    { value: 'python', label: 'Python Project' },
    { value: 'custom', label: 'Custom' }
  ];

  constructor(
    private requestService: ALaCarteRequestService,
    private router: Router
  ) {}

  onBrowseDirectory(): void {
    console.log('Browse directory');
  }

  onAddRepository(): void {
    console.log('Add repository');
  }

  onEditRepository(repo: RepositoryConfig): void {
    console.log('Edit repository', repo);
  }

  onDeleteRepository(repo: RepositoryConfig): void {
    this.repositories = this.repositories.filter(r => r.id !== repo.id);
  }

  onCancel(): void {
    this.router.navigate(['/requests']);
  }

  onSave(): void {
    if (!this.requestName || !this.solutionName || !this.outputDirectory) {
      alert('Please fill in all required fields');
      return;
    }

    this.requestService.createRequest({
      name: this.requestName,
      solutionName: this.solutionName,
      outputDirectory: this.outputDirectory,
      outputType: this.outputType,
      repositories: this.repositories,
      status: 'ready'
    }).subscribe(request => {
      this.router.navigate(['/requests', request.id]);
    });
  }

  getRepositoryTypeLabel(type: string): string {
    return type === 'github' ? 'Git Repository' : 'Local directory';
  }
}
