import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProgressBar } from './progress-bar';

describe('ProgressBar', () => {
  let component: ProgressBar;
  let fixture: ComponentFixture<ProgressBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProgressBar],
    }).compileComponents();

    fixture = TestBed.createComponent(ProgressBar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display default value of 0', () => {
    fixture.detectChanges();
    const fillElement = fixture.nativeElement.querySelector(
      '.progress-bar__fill'
    );
    expect(fillElement.style.width).toBe('0%');
  });

  it('should display the correct progress value', () => {
    fixture.componentRef.setInput('value', 60);
    fixture.detectChanges();

    const fillElement = fixture.nativeElement.querySelector(
      '.progress-bar__fill'
    );
    expect(fillElement.style.width).toBe('60%');
  });

  it('should display percentage by default', () => {
    fixture.componentRef.setInput('value', 45);
    fixture.detectChanges();

    const percentageElement = fixture.nativeElement.querySelector(
      '.progress-bar__percentage'
    );
    expect(percentageElement.textContent.trim()).toBe('45% Complete');
  });

  it('should hide percentage when showPercentage is false', () => {
    fixture.componentRef.setInput('showPercentage', false);
    fixture.detectChanges();

    const percentageElement = fixture.nativeElement.querySelector(
      '.progress-bar__percentage'
    );
    expect(percentageElement).toBeFalsy();
  });

  it('should display label when provided', () => {
    fixture.componentRef.setInput('label', 'Overall Progress');
    fixture.detectChanges();

    const labelElement = fixture.nativeElement.querySelector(
      '.progress-bar__label'
    );
    expect(labelElement.textContent.trim()).toBe('Overall Progress');
  });

  it('should apply animated class by default', () => {
    fixture.detectChanges();
    const fillElement = fixture.nativeElement.querySelector(
      '.progress-bar__fill'
    );
    expect(
      fillElement.classList.contains('progress-bar__fill--animated')
    ).toBe(true);
  });

  it('should not apply animated class when animated is false', () => {
    fixture.componentRef.setInput('animated', false);
    fixture.detectChanges();

    const fillElement = fixture.nativeElement.querySelector(
      '.progress-bar__fill'
    );
    expect(
      fillElement.classList.contains('progress-bar__fill--animated')
    ).toBe(false);
  });
});
