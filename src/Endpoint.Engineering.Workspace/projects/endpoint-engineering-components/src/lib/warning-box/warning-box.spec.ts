import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WarningBox } from './warning-box';

describe('WarningBox', () => {
  let component: WarningBox;
  let fixture: ComponentFixture<WarningBox>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WarningBox],
    }).compileComponents();

    fixture = TestBed.createComponent(WarningBox);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('message', 'This is a warning message');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the message', () => {
    fixture.detectChanges();
    const messageElement = fixture.nativeElement.querySelector('.warning-box__message');
    expect(messageElement.textContent.trim()).toBe('This is a warning message');
  });

  it('should display title when provided', () => {
    fixture.componentRef.setInput('title', 'Warning:');
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('.warning-box__title');
    expect(titleElement.textContent.trim()).toBe('Warning:');
  });

  it('should not display title when not provided', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('.warning-box__title');
    expect(titleElement).toBeFalsy();
  });

  it('should apply warning variant by default', () => {
    fixture.detectChanges();
    const warningBox = fixture.nativeElement.querySelector('.warning-box');
    expect(warningBox.classList.contains('warning-box--warning')).toBe(true);
  });

  it('should apply error variant', () => {
    fixture.componentRef.setInput('variant', 'error');
    fixture.detectChanges();

    const warningBox = fixture.nativeElement.querySelector('.warning-box');
    expect(warningBox.classList.contains('warning-box--error')).toBe(true);
  });

  it('should apply info variant', () => {
    fixture.componentRef.setInput('variant', 'info');
    fixture.detectChanges();

    const warningBox = fixture.nativeElement.querySelector('.warning-box');
    expect(warningBox.classList.contains('warning-box--info')).toBe(true);
  });

  it('should apply success variant', () => {
    fixture.componentRef.setInput('variant', 'success');
    fixture.detectChanges();

    const warningBox = fixture.nativeElement.querySelector('.warning-box');
    expect(warningBox.classList.contains('warning-box--success')).toBe(true);
  });

  it('should return correct icon for warning', () => {
    expect(component.getIcon()).toBe('warning_amber');
  });

  it('should return correct icon for error', () => {
    fixture.componentRef.setInput('variant', 'error');
    fixture.detectChanges();
    expect(component.getIcon()).toBe('error_outline');
  });

  it('should return correct icon for info', () => {
    fixture.componentRef.setInput('variant', 'info');
    fixture.detectChanges();
    expect(component.getIcon()).toBe('info_outline');
  });

  it('should return correct icon for success', () => {
    fixture.componentRef.setInput('variant', 'success');
    fixture.detectChanges();
    expect(component.getIcon()).toBe('check_circle_outline');
  });
});
