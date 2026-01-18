import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LogOutput, LogEntry } from './log-output';

describe('LogOutput', () => {
  let component: LogOutput;
  let fixture: ComponentFixture<LogOutput>;
  const mockLogs: LogEntry[] = [
    { level: 'info', message: 'Starting execution' },
    { level: 'success', message: 'Repository cloned successfully' },
    { level: 'warning', message: 'Missing optional dependency' },
    { level: 'error', message: 'Failed to copy file' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogOutput],
    }).compileComponents();

    fixture = TestBed.createComponent(LogOutput);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('logs', mockLogs);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all log entries', () => {
    fixture.detectChanges();
    const logLines = fixture.nativeElement.querySelectorAll('.log-output__line');
    expect(logLines.length).toBe(4);
  });

  it('should display title when provided', () => {
    fixture.componentRef.setInput('title', 'Execution Log');
    fixture.detectChanges();

    const header = fixture.nativeElement.querySelector('.log-output__header');
    expect(header.textContent.trim()).toContain('Execution Log');
  });

  it('should apply correct class for info level', () => {
    fixture.detectChanges();
    const infoLine = fixture.nativeElement.querySelector('.log-output__line--info');
    expect(infoLine).toBeTruthy();
  });

  it('should apply correct class for success level', () => {
    fixture.detectChanges();
    const successLine = fixture.nativeElement.querySelector('.log-output__line--success');
    expect(successLine).toBeTruthy();
  });

  it('should apply correct class for warning level', () => {
    fixture.detectChanges();
    const warningLine = fixture.nativeElement.querySelector('.log-output__line--warning');
    expect(warningLine).toBeTruthy();
  });

  it('should apply correct class for error level', () => {
    fixture.detectChanges();
    const errorLine = fixture.nativeElement.querySelector('.log-output__line--error');
    expect(errorLine).toBeTruthy();
  });

  it('should return correct level prefix', () => {
    expect(component.getLevelPrefix('info')).toBe('[INFO]');
    expect(component.getLevelPrefix('success')).toBe('[SUCCESS]');
    expect(component.getLevelPrefix('warning')).toBe('[WARNING]');
    expect(component.getLevelPrefix('error')).toBe('[ERROR]');
  });

  it('should show empty message when no logs', () => {
    fixture.componentRef.setInput('logs', []);
    fixture.detectChanges();

    const emptyMessage = fixture.nativeElement.querySelector('.log-output__empty');
    expect(emptyMessage.textContent.trim()).toBe('No log entries');
  });

  it('should apply custom max height', () => {
    fixture.componentRef.setInput('maxHeight', '200px');
    fixture.detectChanges();

    const container = fixture.nativeElement.querySelector('.log-output__container');
    expect(container.style.maxHeight).toBe('200px');
  });
});
