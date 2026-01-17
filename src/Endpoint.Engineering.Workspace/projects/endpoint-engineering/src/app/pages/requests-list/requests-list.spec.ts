import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { RequestsList } from './requests-list';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest } from '../../models/alacarte-request.model';

describe('RequestsList', () => {
  let component: RequestsList;
  let fixture: ComponentFixture<RequestsList>;
  let mockRequestService: jasmine.SpyObj<ALaCarteRequestService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockRequests: ALaCarteRequest[] = [
    {
      id: '1',
      name: 'Test Request 1',
      solutionName: 'TestSolution1',
      outputDirectory: '/test/output1',
      outputType: 'dotnet',
      description: 'Test description 1',
      repositories: [],
      status: 'ready',
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date('2024-01-01')
    },
    {
      id: '2',
      name: 'Test Request 2',
      solutionName: 'TestSolution2',
      outputDirectory: '/test/output2',
      outputType: 'angular',
      repositories: [],
      status: 'draft',
      createdAt: new Date('2024-01-02'),
      updatedAt: new Date('2024-01-02')
    }
  ];

  beforeEach(async () => {
    mockRequestService = jasmine.createSpyObj('ALaCarteRequestService', [
      'getRequests',
      'deleteRequest'
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockRequestService.getRequests.and.returnValue(of(mockRequests));
    mockRequestService.deleteRequest.and.returnValue(of(true));

    await TestBed.configureTestingModule({
      imports: [RequestsList],
      providers: [
        { provide: ALaCarteRequestService, useValue: mockRequestService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestsList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load requests on init', () => {
    expect(mockRequestService.getRequests).toHaveBeenCalled();
    expect(component.requests).toEqual(mockRequests);
    expect(component.filteredRequests).toEqual(mockRequests);
  });

  it('should filter requests by search query', () => {
    component.onSearch('Test Request 1');
    expect(component.filteredRequests.length).toBe(1);
    expect(component.filteredRequests[0].name).toBe('Test Request 1');
  });

  it('should show all requests when search query is empty', () => {
    component.onSearch('Test');
    expect(component.filteredRequests.length).toBe(2);
    component.onSearch('');
    expect(component.filteredRequests.length).toBe(2);
  });

  it('should navigate to create page on create new', () => {
    component.onCreateNew();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/requests/create']);
  });

  it('should navigate to view page on view request', () => {
    component.onViewRequest(mockRequests[0]);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/requests', '1']);
  });

  it('should navigate to edit page on edit request', () => {
    const event = new Event('click');
    spyOn(event, 'stopPropagation');
    component.onEditRequest(mockRequests[0], event);
    expect(event.stopPropagation).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/requests', '1', 'edit']);
  });

  it('should navigate to execute page on execute request', () => {
    const event = new Event('click');
    component.onExecuteRequest(mockRequests[0], event);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/requests', '1', 'execute']);
  });

  it('should delete request after confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    const event = new Event('click');
    component.onDeleteRequest(mockRequests[0], event);
    expect(mockRequestService.deleteRequest).toHaveBeenCalledWith('1');
  });

  it('should not delete request if not confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    const event = new Event('click');
    component.onDeleteRequest(mockRequests[0], event);
    expect(mockRequestService.deleteRequest).not.toHaveBeenCalled();
  });

  it('should return correct status label', () => {
    expect(component.getStatusLabel('draft')).toBe('Draft');
    expect(component.getStatusLabel('ready')).toBe('Ready');
    expect(component.getStatusLabel('executing')).toBe('Executing');
    expect(component.getStatusLabel('completed')).toBe('Completed');
    expect(component.getStatusLabel('failed')).toBe('Failed');
  });

  it('should return correct status type', () => {
    expect(component.getStatusType('draft')).toBe('info');
    expect(component.getStatusType('ready')).toBe('success');
    expect(component.getStatusType('executing')).toBe('warning');
    expect(component.getStatusType('completed')).toBe('success');
    expect(component.getStatusType('failed')).toBe('error');
  });

  it('should format date correctly', () => {
    const date = new Date('2024-01-15T10:30:00');
    const formatted = component.formatDate(date);
    expect(formatted).toContain('Jan');
    expect(formatted).toContain('15');
    expect(formatted).toContain('2024');
  });
});
