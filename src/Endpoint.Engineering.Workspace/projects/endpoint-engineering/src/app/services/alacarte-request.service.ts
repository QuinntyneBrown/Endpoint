import { Injectable, signal } from '@angular/core';
import { ALaCarteRequest, ALaCarteRequestExecution, OutputType } from '../models/alacarte-request.model';

@Injectable({
  providedIn: 'root'
})
export class ALaCarteRequestService {
  private readonly requests = signal<ALaCarteRequest[]>([]);
  private readonly currentExecution = signal<ALaCarteRequestExecution | null>(null);

  readonly requests$ = this.requests.asReadonly();
  readonly currentExecution$ = this.currentExecution.asReadonly();

  constructor() {
    // Initialize with some mock data for development
    this.loadMockData();
  }

  getAll(): ALaCarteRequest[] {
    return this.requests();
  }

  getById(id: string): ALaCarteRequest | undefined {
    return this.requests().find(r => r.id === id);
  }

  create(request: Omit<ALaCarteRequest, 'id' | 'createdAt' | 'executionCount'>): ALaCarteRequest {
    const newRequest: ALaCarteRequest = {
      ...request,
      id: this.generateId(),
      createdAt: new Date(),
      executionCount: 0
    };
    this.requests.update(requests => [...requests, newRequest]);
    return newRequest;
  }

  update(id: string, updates: Partial<ALaCarteRequest>): void {
    this.requests.update(requests =>
      requests.map(r => r.id === id ? { ...r, ...updates } : r)
    );
  }

  delete(id: string): void {
    this.requests.update(requests => requests.filter(r => r.id !== id));
  }

  execute(id: string): void {
    const request = this.getById(id);
    if (!request) return;

    this.update(id, {
      lastUsed: new Date(),
      executionCount: request.executionCount + 1
    });

    const execution: ALaCarteRequestExecution = {
      requestId: id,
      startedAt: new Date(),
      status: 'in_progress',
      logs: [{
        timestamp: new Date(),
        level: 'info',
        message: `Starting execution of "${request.name}"...`
      }]
    };

    this.currentExecution.set(execution);

    // Simulate execution (in a real app, this would call a backend API)
    this.simulateExecution(execution);
  }

  private simulateExecution(execution: ALaCarteRequestExecution): void {
    const stages = ['Initialize', 'Clone Repositories', 'Copy Folders', 'Create Solution', 'Finalize'];
    let currentStageIndex = 0;

    const interval = setInterval(() => {
      if (currentStageIndex >= stages.length) {
        clearInterval(interval);
        this.currentExecution.update(exec => exec ? {
          ...exec,
          status: 'completed',
          completedAt: new Date(),
          logs: [...exec.logs, {
            timestamp: new Date(),
            level: 'success',
            message: 'Execution completed successfully!'
          }]
        } : null);
        return;
      }

      const stage = stages[currentStageIndex];
      this.currentExecution.update(exec => exec ? {
        ...exec,
        currentStage: stage,
        logs: [...exec.logs, {
          timestamp: new Date(),
          level: 'info',
          message: `Processing stage: ${stage}...`
        }]
      } : null);

      currentStageIndex++;
    }, 2000);
  }

  private generateId(): string {
    return `alacarte-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private loadMockData(): void {
    const mockRequests: ALaCarteRequest[] = [
      {
        id: '1',
        name: 'Backend API Project',
        solutionName: 'MyApi',
        outputDirectory: '/projects/my-api',
        outputType: OutputType.DotNetSolution,
        repositories: [
          {
            url: 'https://github.com/example/backend-templates',
            branch: 'main',
            folders: [
              { sourcePath: 'src/Core', destinationPath: 'src/Core' },
              { sourcePath: 'src/Api', destinationPath: 'src/Api' }
            ]
          }
        ],
        createdAt: new Date('2024-01-15'),
        lastUsed: new Date('2024-01-20'),
        executionCount: 5
      },
      {
        id: '2',
        name: 'Frontend Angular App',
        solutionName: 'MyAngularApp',
        outputDirectory: '/projects/my-angular-app',
        outputType: OutputType.AngularWorkspace,
        repositories: [
          {
            url: 'https://github.com/example/angular-templates',
            branch: 'develop',
            folders: [
              { sourcePath: 'projects/shared', destinationPath: 'projects/shared' }
            ]
          }
        ],
        createdAt: new Date('2024-01-10'),
        lastUsed: new Date('2024-01-18'),
        executionCount: 3
      }
    ];

    this.requests.set(mockRequests);
  }
}
