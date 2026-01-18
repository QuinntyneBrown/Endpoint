import { TestBed } from '@angular/core/testing';
import { ALaCarteRequestService } from './alacarte-request.service';
import { OutputType } from '../models/alacarte-request.model';

describe('ALaCarteRequestService', () => {
  let service: ALaCarteRequestService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ALaCarteRequestService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load mock data on initialization', () => {
    const requests = service.getAll();
    expect(requests.length).toBeGreaterThan(0);
  });

  it('should get all requests', () => {
    const requests = service.getAll();
    expect(Array.isArray(requests)).toBe(true);
  });

  it('should get request by id', () => {
    const requests = service.getAll();
    const firstRequest = requests[0];
    const found = service.getById(firstRequest.id);
    expect(found).toEqual(firstRequest);
  });

  it('should return undefined for non-existent id', () => {
    const found = service.getById('non-existent-id');
    expect(found).toBeUndefined();
  });

  it('should create a new request', () => {
    const initialCount = service.getAll().length;
    const newRequest = {
      name: 'Test Request',
      solutionName: 'TestSolution',
      outputDirectory: '/test',
      outputType: OutputType.DotNetSolution,
      repositories: []
    };

    const created = service.create(newRequest);

    expect(created.id).toBeDefined();
    expect(created.name).toBe(newRequest.name);
    expect(created.createdAt).toBeInstanceOf(Date);
    expect(created.executionCount).toBe(0);
    expect(service.getAll().length).toBe(initialCount + 1);
  });

  it('should update an existing request', () => {
    const requests = service.getAll();
    const requestToUpdate = requests[0];
    const newName = 'Updated Name';

    service.update(requestToUpdate.id, { name: newName });

    const updated = service.getById(requestToUpdate.id);
    expect(updated?.name).toBe(newName);
  });

  it('should delete a request', () => {
    const initialCount = service.getAll().length;
    const requests = service.getAll();
    const requestToDelete = requests[0];

    service.delete(requestToDelete.id);

    expect(service.getAll().length).toBe(initialCount - 1);
    expect(service.getById(requestToDelete.id)).toBeUndefined();
  });

  it('should execute a request', () => {
    const requests = service.getAll();
    const requestToExecute = requests[0];
    const initialExecutionCount = requestToExecute.executionCount;

    service.execute(requestToExecute.id);

    const updated = service.getById(requestToExecute.id);
    expect(updated?.executionCount).toBe(initialExecutionCount + 1);
    expect(updated?.lastUsed).toBeInstanceOf(Date);
    expect(service.currentExecution$()).toBeTruthy();
  });

  it('should not execute non-existent request', () => {
    service.execute('non-existent-id');
    // Should not throw error and execution should remain null
    expect(service.currentExecution$()).toBeNull();
  });
});
