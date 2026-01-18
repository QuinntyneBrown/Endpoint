import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { RequestCreatePage } from './request-create';
import { ALaCarteRequestService } from '../../services/alacarte-request.service';
import { OutputType } from '../../models/alacarte-request.model';

describe('RequestCreatePage', () => {
  let component: RequestCreatePage;
  let fixture: ComponentFixture<RequestCreatePage>;
  let service: ALaCarteRequestService;
  let router: Router;

  beforeEach(async () => {
    const routerSpy = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [RequestCreatePage],
      providers: [
        ALaCarteRequestService,
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestCreatePage);
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

  it('should have form fields', () => {
    const compiled = fixture.nativeElement;
    const formFields = compiled.querySelectorAll('mat-form-field');
    expect(formFields.length).toBeGreaterThan(0);
  });

  it('should be invalid initially', () => {
    expect(component.isValid()).toBe(false);
  });

  it('should be valid when all required fields are filled', () => {
    component['name'].set('Test Request');
    component['solutionName'].set('TestSolution');
    component['outputDirectory'].set('/test');
    expect(component.isValid()).toBe(true);
  });

  it('should navigate to requests list on cancel', () => {
    component.onCancel();
    expect(router.navigate).toHaveBeenCalledWith(['/requests']);
  });

  it('should create request and navigate on save', () => {
    const initialCount = service.getAll().length;
    
    component['name'].set('Test Request');
    component['solutionName'].set('TestSolution');
    component['outputDirectory'].set('/test');
    component['outputType'].set(OutputType.DotNetSolution);

    component.onSave();

    expect(service.getAll().length).toBe(initialCount + 1);
    expect(router.navigate).toHaveBeenCalledWith(['/requests']);
  });

  it('should have all output types available', () => {
    expect(component['outputTypes'].length).toBe(Object.values(OutputType).length);
  });
});
