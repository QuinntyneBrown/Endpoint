import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WizardSteps, WizardStep } from './wizard-steps';

describe('WizardSteps', () => {
  let component: WizardSteps;
  let fixture: ComponentFixture<WizardSteps>;
  const mockSteps: WizardStep[] = [
    { label: 'Basic Info' },
    { label: 'Select Components' },
    { label: 'Configure' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WizardSteps],
    }).compileComponents();

    fixture = TestBed.createComponent(WizardSteps);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('steps', mockSteps);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all steps', () => {
    fixture.detectChanges();
    const stepElements = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__step'
    );
    expect(stepElements.length).toBe(3);
  });

  it('should display step labels', () => {
    fixture.detectChanges();
    const labels = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__label'
    );
    expect(labels[0].textContent.trim()).toBe('Basic Info');
    expect(labels[1].textContent.trim()).toBe('Select Components');
    expect(labels[2].textContent.trim()).toBe('Configure');
  });

  it('should mark first step as active by default', () => {
    fixture.detectChanges();
    const steps = fixture.nativeElement.querySelectorAll('.wizard-steps__step');
    expect(steps[0].classList.contains('wizard-steps__step--active')).toBe(
      true
    );
  });

  it('should mark the correct step as active', () => {
    fixture.componentRef.setInput('currentStep', 1);
    fixture.detectChanges();

    const steps = fixture.nativeElement.querySelectorAll('.wizard-steps__step');
    expect(steps[0].classList.contains('wizard-steps__step--completed')).toBe(
      true
    );
    expect(steps[1].classList.contains('wizard-steps__step--active')).toBe(
      true
    );
    expect(steps[2].classList.contains('wizard-steps__step--active')).toBe(
      false
    );
  });

  it('should show step numbers in indicators', () => {
    fixture.detectChanges();
    const indicators = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__indicator'
    );
    expect(indicators[1].textContent.trim()).toBe('2');
    expect(indicators[2].textContent.trim()).toBe('3');
  });

  it('should show check icon for completed steps', () => {
    fixture.componentRef.setInput('currentStep', 2);
    fixture.detectChanges();

    const checkIcons = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__check'
    );
    expect(checkIcons.length).toBe(2);
  });

  it('should render connectors between steps', () => {
    fixture.detectChanges();
    const connectors = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__connector'
    );
    expect(connectors.length).toBe(2);
  });

  it('should mark connectors as completed for past steps', () => {
    fixture.componentRef.setInput('currentStep', 2);
    fixture.detectChanges();

    const connectors = fixture.nativeElement.querySelectorAll(
      '.wizard-steps__connector'
    );
    expect(
      connectors[0].classList.contains('wizard-steps__connector--completed')
    ).toBe(true);
    expect(
      connectors[1].classList.contains('wizard-steps__connector--completed')
    ).toBe(true);
  });
});
