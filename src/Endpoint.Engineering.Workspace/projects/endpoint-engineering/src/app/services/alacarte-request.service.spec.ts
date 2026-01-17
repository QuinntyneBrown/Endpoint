import { TestBed } from '@angular/core/testing';
import { ALaCarteRequestService } from './alacarte-request.service';
import { ALaCarteRequest, Configuration } from '../models/alacarte-request.model';

describe('ALaCarteRequestService', () => {
  let service: ALaCarteRequestService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ALaCarteRequestService]
    });
    service = TestBed.inject(ALaCarteRequestService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getRequests', () => {
    it('should return requests observable', (done) => {
      service.getRequests().subscribe(requests => {
        expect(requests).toBeDefined();
        expect(Array.isArray(requests)).toBe(true);
        expect(requests.length).toBeGreaterThan(0);
        done();
      });
    });
  });

  describe('getRequest', () => {
    it('should return a specific request', (done) => {
      service.getRequests().subscribe(requests => {
        const firstRequestId = requests[0].id;
        service.getRequest(firstRequestId).subscribe(request => {
          expect(request).toBeDefined();
          expect(request?.id).toBe(firstRequestId);
          done();
        });
      });
    });

    it('should return undefined for non-existent request', (done) => {
      service.getRequest('non-existent-id').subscribe(request => {
        expect(request).toBeUndefined();
        done();
      });
    });
  });

  describe('createRequest', () => {
    it('should create a new request', (done) => {
      const newRequest = {
        name: 'New Request',
        solutionName: 'NewSolution',
        outputDirectory: '/test/output',
        outputType: 'dotnet' as const,
        repositories: [],
        status: 'draft' as const
      };

      service.createRequest(newRequest).subscribe(created => {
        expect(created).toBeDefined();
        expect(created.id).toBeDefined();
        expect(created.name).toBe('New Request');
        expect(created.createdAt).toBeDefined();
        expect(created.updatedAt).toBeDefined();
        done();
      });
    });

    it('should add the new request to the list', (done) => {
      const newRequest = {
        name: 'Test Request',
        solutionName: 'TestSolution',
        outputDirectory: '/test',
        outputType: 'angular' as const,
        repositories: [],
        status: 'ready' as const
      };

      let initialCount = 0;
      service.getRequests().subscribe(requests => {
        initialCount = requests.length;
      });

      service.createRequest(newRequest).subscribe(() => {
        service.getRequests().subscribe(requests => {
          expect(requests.length).toBe(initialCount + 1);
          const created = requests.find(r => r.name === 'Test Request');
          expect(created).toBeDefined();
          done();
        });
      });
    });
  });

  describe('updateRequest', () => {
    it('should update an existing request', (done) => {
      service.getRequests().subscribe(requests => {
        const requestId = requests[0].id;
        const updates = { name: 'Updated Name' };

        service.updateRequest(requestId, updates).subscribe(updated => {
          expect(updated).toBeDefined();
          expect(updated?.name).toBe('Updated Name');
          expect(updated?.updatedAt).toBeDefined();
          done();
        });
      });
    });

    it('should return undefined for non-existent request', (done) => {
      service.updateRequest('non-existent-id', { name: 'Test' }).subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });
  });

  describe('deleteRequest', () => {
    it('should delete a request', (done) => {
      let initialCount = 0;
      service.getRequests().subscribe(requests => {
        initialCount = requests.length;
        const requestId = requests[0].id;

        service.deleteRequest(requestId).subscribe(success => {
          expect(success).toBe(true);
          service.getRequests().subscribe(updatedRequests => {
            expect(updatedRequests.length).toBe(initialCount - 1);
            const deleted = updatedRequests.find(r => r.id === requestId);
            expect(deleted).toBeUndefined();
            done();
          });
        });
      });
    });
  });

  describe('executeRequest', () => {
    it('should execute a request and return result', (done) => {
      service.executeRequest('test-id').subscribe(result => {
        expect(result).toBeDefined();
        expect(result.requestId).toBe('test-id');
        expect(result.status).toBe('success');
        expect(result.outputPath).toBeDefined();
        expect(result.executionTime).toBeDefined();
        done();
      });
    });
  });

  describe('getConfigurations', () => {
    it('should return configurations observable', (done) => {
      service.getConfigurations().subscribe(configs => {
        expect(configs).toBeDefined();
        expect(Array.isArray(configs)).toBe(true);
        done();
      });
    });
  });

  describe('saveConfiguration', () => {
    it('should save a new configuration', (done) => {
      const newConfig = {
        name: 'Test Config',
        description: 'Test description',
        repositories: []
      };

      service.saveConfiguration(newConfig).subscribe(saved => {
        expect(saved).toBeDefined();
        expect(saved.id).toBeDefined();
        expect(saved.name).toBe('Test Config');
        expect(saved.createdAt).toBeDefined();
        expect(saved.updatedAt).toBeDefined();
        done();
      });
    });
  });

  describe('loadConfiguration', () => {
    it('should load a specific configuration', (done) => {
      service.getConfigurations().subscribe(configs => {
        if (configs.length > 0) {
          const configId = configs[0].id;
          service.loadConfiguration(configId).subscribe(config => {
            expect(config).toBeDefined();
            expect(config?.id).toBe(configId);
            done();
          });
        } else {
          done();
        }
      });
    });

    it('should return undefined for non-existent configuration', (done) => {
      service.loadConfiguration('non-existent-id').subscribe(config => {
        expect(config).toBeUndefined();
        done();
      });
    });
  });
});
