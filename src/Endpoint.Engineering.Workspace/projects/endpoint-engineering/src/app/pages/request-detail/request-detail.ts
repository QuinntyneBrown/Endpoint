import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import {
  Button,
  PageHeader,
  JsonPreview
} from 'endpoint-engineering-components';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest, ALaCarteRequestExecution } from '../../models/alacarte-request.model';

@Component({
  selector: 'app-request-detail',
  imports: [
    CommonModule,
    MatIconModule,
    MatTabsModule,
    Button,
    PageHeader,
    JsonPreview
  ],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.scss'
})
export class RequestDetailPage implements OnInit {
  protected readonly request = signal<ALaCarteRequest | null>(null);
  protected readonly execution = signal<ALaCarteRequestExecution | null>(null);
  protected readonly loading = signal(true);
  protected readonly activeTab = signal(0);

  protected readonly requestJson = computed(() => {
    const req = this.request();
    if (!req) return {};
    return {
      name: req.name,
      solutionName: req.solutionName,
      outputDirectory: req.outputDirectory,
      outputType: req.outputType,
      repositories: req.repositories
    };
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private requestService: ALaCarteRequestService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      const req = this.requestService.getById(id);
      this.request.set(req || null);
    }
    this.loading.set(false);
  }

  onEdit(): void {
    const req = this.request();
    if (req) {
      this.router.navigate(['/request', req.id, 'edit']);
    }
  }

  onExecute(): void {
    const req = this.request();
    if (req) {
      this.requestService.execute(req.id);
      // Navigate to execution view
      this.activeTab.set(1);
    }
  }

  onDelete(): void {
    const req = this.request();
    if (req) {
      this.requestService.delete(req.id);
      this.router.navigate(['/requests']);
    }
  }

  onBack(): void {
    this.router.navigate(['/requests']);
  }
}
