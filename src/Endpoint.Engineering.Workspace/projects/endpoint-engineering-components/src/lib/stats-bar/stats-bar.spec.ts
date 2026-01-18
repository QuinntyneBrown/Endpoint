import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatsBar, StatItem } from './stats-bar';

describe('StatsBar', () => {
  let component: StatsBar;
  let fixture: ComponentFixture<StatsBar>;
  const mockStats: StatItem[] = [
    { value: 12, label: 'Saved Configs' },
    { value: 48, label: 'Total Repos' },
    { value: 127, label: 'Components' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatsBar],
    }).compileComponents();

    fixture = TestBed.createComponent(StatsBar);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('stats', mockStats);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all stats', () => {
    fixture.detectChanges();
    const statElements = fixture.nativeElement.querySelectorAll(
      '.stats-bar__stat'
    );
    expect(statElements.length).toBe(3);
  });

  it('should display stat values', () => {
    fixture.detectChanges();
    const valueElements = fixture.nativeElement.querySelectorAll(
      '.stats-bar__value'
    );
    expect(valueElements[0].textContent.trim()).toBe('12');
    expect(valueElements[1].textContent.trim()).toBe('48');
    expect(valueElements[2].textContent.trim()).toBe('127');
  });

  it('should display stat labels', () => {
    fixture.detectChanges();
    const labelElements = fixture.nativeElement.querySelectorAll(
      '.stats-bar__label'
    );
    expect(labelElements[0].textContent.trim()).toBe('Saved Configs');
    expect(labelElements[1].textContent.trim()).toBe('Total Repos');
    expect(labelElements[2].textContent.trim()).toBe('Components');
  });

  it('should handle string values', () => {
    fixture.componentRef.setInput('stats', [
      { value: '1.2K', label: 'Downloads' },
    ]);
    fixture.detectChanges();

    const valueElement = fixture.nativeElement.querySelector(
      '.stats-bar__value'
    );
    expect(valueElement.textContent.trim()).toBe('1.2K');
  });
});
