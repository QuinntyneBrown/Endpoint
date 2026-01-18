import { ComponentFixture, TestBed } from '@angular/core/testing';
import { QuickOption } from './quick-option';

describe('QuickOption', () => {
  let component: QuickOption;
  let fixture: ComponentFixture<QuickOption>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuickOption],
    }).compileComponents();

    fixture = TestBed.createComponent(QuickOption);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'cloud');
    fixture.componentRef.setInput('title', 'From GitHub');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the icon', () => {
    fixture.detectChanges();
    const iconElement = fixture.nativeElement.querySelector(
      '.quick-option__icon .material-icons'
    );
    expect(iconElement.textContent.trim()).toBe('cloud');
  });

  it('should display the title', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector(
      '.quick-option__title'
    );
    expect(titleElement.textContent.trim()).toBe('From GitHub');
  });

  it('should display description when provided', () => {
    fixture.componentRef.setInput('description', 'Browse repository folders');
    fixture.detectChanges();

    const descriptionElement = fixture.nativeElement.querySelector(
      '.quick-option__description'
    );
    expect(descriptionElement.textContent.trim()).toBe(
      'Browse repository folders'
    );
  });

  it('should not display description when not provided', () => {
    fixture.detectChanges();
    const descriptionElement = fixture.nativeElement.querySelector(
      '.quick-option__description'
    );
    expect(descriptionElement).toBeFalsy();
  });

  it('should emit selected event when clicked', () => {
    fixture.detectChanges();
    const selectSpy = vi.fn();
    component.selected.subscribe(selectSpy);

    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(selectSpy).toHaveBeenCalled();
  });
});
