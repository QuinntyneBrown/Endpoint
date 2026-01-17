import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, delay } from 'rxjs';
import { ALaCarteRequest, Configuration, ExecutionResult } from '../models/alacarte-request.model';

@Injectable({
  providedIn: 'root'
})
export class ALaCarteRequestService {
  private requests = new BehaviorSubject<ALaCarteRequest[]>([]);
  private configurations = new BehaviorSubject<Configuration[]>([]);

  constructor() {
    this.loadMockData();
  }

  getRequests(): Observable<ALaCarteRequest[]> {
    return this.requests.asObservable();
  }

  getRequest(id: string): Observable<ALaCarteRequest | undefined> {
    return of(this.requests.value.find(r => r.id === id));
  }

  createRequest(request: Omit<ALaCarteRequest, 'id' | 'createdAt' | 'updatedAt'>): Observable<ALaCarteRequest> {
    const newRequest: ALaCarteRequest = {
      ...request,
      id: this.generateId(),
      createdAt: new Date(),
      updatedAt: new Date()
    };
    this.requests.next([...this.requests.value, newRequest]);
    return of(newRequest);
  }

  updateRequest(id: string, updates: Partial<ALaCarteRequest>): Observable<ALaCarteRequest | undefined> {
    const requests = this.requests.value;
    const index = requests.findIndex(r => r.id === id);
    if (index === -1) return of(undefined);
    
    const updatedRequest = {
      ...requests[index],
      ...updates,
      updatedAt: new Date()
    };
    requests[index] = updatedRequest;
    this.requests.next([...requests]);
    return of(updatedRequest);
  }

  deleteRequest(id: string): Observable<boolean> {
    const requests = this.requests.value.filter(r => r.id !== id);
    this.requests.next(requests);
    return of(true).pipe(delay(300));
  }

  executeRequest(id: string): Observable<ExecutionResult> {
    return of({
      requestId: id,
      status: 'success' as const,
      outputPath: '/output/MyApiSolution',
      generatedFiles: ['MyApiSolution.sln', 'src/MyApiSolution.Api/Program.cs'],
      executionTime: 2500
    }).pipe(delay(2500));
  }

  getConfigurations(): Observable<Configuration[]> {
    return this.configurations.asObservable();
  }

  saveConfiguration(config: Omit<Configuration, 'id' | 'createdAt' | 'updatedAt'>): Observable<Configuration> {
    const newConfig: Configuration = {
      ...config,
      id: this.generateId(),
      createdAt: new Date(),
      updatedAt: new Date()
    };
    this.configurations.next([...this.configurations.value, newConfig]);
    return of(newConfig);
  }

  loadConfiguration(id: string): Observable<Configuration | undefined> {
    return of(this.configurations.value.find(c => c.id === id));
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private loadMockData(): void {
    const mockRequests: ALaCarteRequest[] = [
      {
        id: '1',
        name: 'E-Commerce API',
        solutionName: 'ECommerceApi',
        outputDirectory: '~/projects/ecommerce-output',
        outputType: 'dotnet',
        description: 'API with product catalog and orders',
        repositories: [
          {
            id: 'r1',
            type: 'github',
            url: 'https://github.com/dotnet/aspnetcore',
            branch: 'main',
            folders: [
              { id: 'f1', path: 'src/Controllers', order: 0 },
              { id: 'f2', path: 'src/Services', order: 1 }
            ],
            order: 0
          }
        ],
        status: 'ready',
        createdAt: new Date('2024-01-15'),
        updatedAt: new Date('2024-01-15')
      },
      {
        id: '2',
        name: 'Angular Dashboard',
        solutionName: 'AdminDashboard',
        outputDirectory: '~/projects/dashboard',
        outputType: 'angular',
        repositories: [],
        status: 'draft',
        createdAt: new Date('2024-01-16'),
        updatedAt: new Date('2024-01-16')
      }
    ];

    const mockConfigs: Configuration[] = [
      {
        id: 'c1',
        name: 'Standard Web API',
        description: 'Common configuration for REST APIs',
        repositories: [
          {
            id: 'r1',
            type: 'github',
            url: 'https://github.com/QuinntyneBrown/Endpoint',
            branch: 'main',
            folders: [
              { id: 'f1', path: 'src/Controllers', order: 0 }
            ],
            order: 0
          }
        ],
        createdAt: new Date('2024-01-10'),
        updatedAt: new Date('2024-01-10')
      }
    ];

    this.requests.next(mockRequests);
    this.configurations.next(mockConfigs);
  }
}
