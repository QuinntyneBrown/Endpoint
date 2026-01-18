import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StageList, Stage } from './stage-list';

describe('StageList', () => {
  let component: StageList;
  let fixture: ComponentFixture<StageList>;
  const mockStages: Stage[] = [
    { name: 'Clone Repositories', status: 'completed', description: '5 of 5 repositories cloned' },
    { name: 'Validate Folders', status: 'completed', description: '9 folders validated' },
    { name: 'Copy Project Folders', status: 'active', description: 'Copying files...' },
    { name: 'Create Solution File', status: 'pending', description: 'Waiting...' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StageList],
    }).compileComponents();

    fixture = TestBed.createComponent(StageList);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('stages', mockStages);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all stages', () => {
    fixture.detectChanges();
    const stageElements = fixture.nativeElement.querySelectorAll('.stage-list__item');
    expect(stageElements.length).toBe(4);
  });

  it('should display stage names', () => {
    fixture.detectChanges();
    const nameElements = fixture.nativeElement.querySelectorAll('.stage-list__name');
    expect(nameElements[0].textContent.trim()).toBe('Clone Repositories');
    expect(nameElements[2].textContent.trim()).toBe('Copy Project Folders');
  });

  it('should display stage descriptions', () => {
    fixture.detectChanges();
    const statusElements = fixture.nativeElement.querySelectorAll('.stage-list__status');
    expect(statusElements[0].textContent.trim()).toBe('5 of 5 repositories cloned');
  });

  it('should apply active class to active stages', () => {
    fixture.detectChanges();
    const stageElements = fixture.nativeElement.querySelectorAll('.stage-list__item');
    expect(stageElements[2].classList.contains('stage-list__item--active')).toBe(true);
  });

  it('should apply completed class to completed stages', () => {
    fixture.detectChanges();
    const stageElements = fixture.nativeElement.querySelectorAll('.stage-list__item');
    expect(stageElements[0].classList.contains('stage-list__item--completed')).toBe(true);
  });

  it('should return correct icon for status', () => {
    expect(component.getIconForStatus('completed')).toBe('check_circle');
    expect(component.getIconForStatus('active')).toBe('hourglass_empty');
    expect(component.getIconForStatus('pending')).toBe('radio_button_unchecked');
    expect(component.getIconForStatus('error')).toBe('error');
  });

  it('should return correct badge variant', () => {
    expect(component.getBadgeVariant('completed')).toBe('success');
    expect(component.getBadgeVariant('active')).toBe('processing');
    expect(component.getBadgeVariant('pending')).toBe('pending');
    expect(component.getBadgeVariant('error')).toBe('error');
  });

  it('should return correct badge label', () => {
    expect(component.getBadgeLabel('completed')).toBe('Completed');
    expect(component.getBadgeLabel('active')).toBe('Processing');
    expect(component.getBadgeLabel('pending')).toBe('Pending');
    expect(component.getBadgeLabel('error')).toBe('Failed');
  });
});
