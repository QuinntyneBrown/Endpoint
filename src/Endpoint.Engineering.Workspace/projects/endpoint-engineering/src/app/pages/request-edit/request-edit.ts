import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Card, FormInput, Badge } from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest, OutputType, RepositoryConfig } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-edit',
  imports: [CommonModule, FormsModule, Card, FormInput, Badge, MatButtonModule, MatIconModule],
  templateUrl: './request-edit.html',
  styleUrl: './request-edit.scss'
})
export class RequestEdit implements OnInit {
  request?: ALaCarteRequest;
  requestName = '';
  solutionName = '';
  outputDirectory = '';
  outputType: OutputType = 'dotnet';
  repositories: RepositoryConfig[] = [];
  loading = true;

  outputTypes: { value: OutputType; label: string }[] = [
    { value: 'dotnet', label: 'DotNet Solution (.sln)' },
    { value: 'angular', label: 'Angular Workspace' },
    { value: 'node', label: 'Node.js Package' },
    { value: 'python', label: 'Python Project' },
    { value: 'custom', label: 'Custom' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private requestService: ALaCarteRequestService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.requestService.getRequest(id).subscribe(request => {
        if (request) {
          this.request = request;
          this.requestName = request.name;
          this.solutionName = request.solutionName;
          this.outputDirectory = request.outputDirectory;
          this.outputType = request.outputType;
          this.repositories = [...request.repositories];
        }
        this.loading = false;
      });
    }
  }

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
    if (this.request) {
      this.router.navigate(['/requests', this.request.id]);
    } else {
      this.router.navigate(['/requests']);
    }
  }

  onSave(): void {
    if (!this.request || !this.requestName || !this.solutionName || !this.outputDirectory) {
      alert('Please fill in all required fields');
      return;
    }

    this.requestService.updateRequest(this.request.id, {
      name: this.requestName,
      solutionName: this.solutionName,
      outputDirectory: this.outputDirectory,
      outputType: this.outputType,
      repositories: this.repositories
    }).subscribe(updated => {
      if (updated) {
        this.router.navigate(['/requests', updated.id]);
      }
    });
  }

  getRepositoryTypeLabel(type: string): string {
    return type === 'github' ? 'Git Repository' : 'Local directory';
  }
}
