import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { RequestListPage } from './request-list';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { ALaCarteRequest, OutputType } from '../../models/alacarte-request.model';

describe('RequestListPage', () => {
  let component: RequestListPage;
  let fixture: ComponentFixture<RequestListPage>;
  let service: ALaCarteRequestService;
  let router: Router;

  beforeEach(async () => {
    const routerSpy = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [RequestListPage],
      providers: [
        ALaCarteRequestService,
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestListPage);
    component = fixture.componentInstance;
    service = TestBed.inject(ALaCarteRequestService);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display page header', () => {
    const compiled = fixture.nativeElement;
    const pageHeader = compiled.querySelector('ee-page-header');
    expect(pageHeader).toBeTruthy();
  });

  it('should display stats bar', () => {
    const compiled = fixture.nativeElement;
    const statsBar = compiled.querySelector('ee-stats-bar');
    expect(statsBar).toBeTruthy();
  });

  it('should display search box', () => {
    const compiled = fixture.nativeElement;
    const searchBox = compiled.querySelector('ee-search-box');
    expect(searchBox).toBeTruthy();
  });

  it('should filter requests on search', () => {
    const requests = service.getAll();
    expect(component['requests']().length).toBe(requests.length);

    component.onSearch('Backend');
    fixture.detectChanges();

    const filtered = component['requests']();
    expect(filtered.length).toBeLessThanOrEqual(requests.length);
    expect(filtered.every(r => 
      r.name.toLowerCase().includes('backend') || 
      r.solutionName.toLowerCase().includes('backend')
    )).toBe(true);
  });

  it('should navigate to create page on new request', () => {
    component.onNewRequest();
    expect(router.navigate).toHaveBeenCalledWith(['/request/create']);
  });

  it('should navigate to view page', () => {
    const request = service.getAll()[0];
    component.onView(request);
    expect(router.navigate).toHaveBeenCalledWith(['/request', request.id]);
  });

  it('should navigate to edit page', () => {
    const request = service.getAll()[0];
    component.onEdit(request);
    expect(router.navigate).toHaveBeenCalledWith(['/request', request.id, 'edit']);
  });

  it('should navigate to execute page', () => {
    const request = service.getAll()[0];
    component.onExecute(request);
    expect(router.navigate).toHaveBeenCalledWith(['/request', request.id, 'execute']);
  });

  it('should delete request after confirmation', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const request = service.getAll()[0];
    const initialCount = service.getAll().length;

    component.onDelete(request);

    expect(service.getAll().length).toBe(initialCount - 1);
    confirmSpy.mockRestore();
  });

  it('should not delete request if not confirmed', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const request = service.getAll()[0];
    const initialCount = service.getAll().length;

    component.onDelete(request);

    expect(service.getAll().length).toBe(initialCount);
    confirmSpy.mockRestore();
  });

  it('should format date correctly', () => {
    const date = new Date('2024-01-15');
    const formatted = component.formatDate(date);
    expect(formatted).toContain('2024');
  });

  it('should return "Never" for undefined date', () => {
    const formatted = component.formatDate(undefined);
    expect(formatted).toBe('Never');
  });

  it('should calculate stats correctly', () => {
    const stats = component['stats']();
    expect(stats.length).toBe(3);
    expect(stats[0].label).toBe('Total Requests');
    expect(stats[1].label).toBe('Executions');
    expect(stats[2].label).toBe('Used This Week');
  });
});
