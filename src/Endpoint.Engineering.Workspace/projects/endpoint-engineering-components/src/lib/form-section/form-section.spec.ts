import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormSection } from './form-section';

describe('FormSection', () => {
  let component: FormSection;
  let fixture: ComponentFixture<FormSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormSection],
    }).compileComponents();

    fixture = TestBed.createComponent(FormSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the title when provided', () => {
    fixture.componentRef.setInput('title', 'Project Information');
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector(
      '.form-section__title'
    );
    expect(titleElement.textContent.trim()).toBe('Project Information');
  });

  it('should not display title when not provided', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector(
      '.form-section__title'
    );
    expect(titleElement).toBeFalsy();
  });

  it('should have content area for child elements', () => {
    fixture.detectChanges();
    const contentElement = fixture.nativeElement.querySelector(
      '.form-section__content'
    );
    expect(contentElement).toBeTruthy();
  });
});
