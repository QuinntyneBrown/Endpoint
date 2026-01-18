import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Button } from './button';

describe('Button', () => {
  let component: Button;
  let fixture: ComponentFixture<Button>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Button],
    }).compileComponents();

    fixture = TestBed.createComponent(Button);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should apply primary variant by default', () => {
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--primary')).toBe(true);
  });

  it('should apply secondary variant', () => {
    fixture.componentRef.setInput('variant', 'secondary');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--secondary')).toBe(true);
  });

  it('should apply danger variant', () => {
    fixture.componentRef.setInput('variant', 'danger');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--danger')).toBe(true);
  });

  it('should emit clicked event when clicked', () => {
    const clickSpy = vi.fn();
    component.clicked.subscribe(clickSpy);

    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickSpy).toHaveBeenCalled();
  });

  it('should not emit clicked when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const clickSpy = vi.fn();
    component.clicked.subscribe(clickSpy);

    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickSpy).not.toHaveBeenCalled();
  });

  it('should display icon when provided', () => {
    fixture.componentRef.setInput('icon', 'add');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('.material-icons');
    expect(icon).toBeTruthy();
    expect(icon.textContent.trim()).toBe('add');
  });

  it('should apply full-width class when fullWidth is true', () => {
    fixture.componentRef.setInput('fullWidth', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--full-width')).toBe(true);
  });

  it('should set button type', () => {
    fixture.componentRef.setInput('type', 'submit');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.type).toBe('submit');
  });

  it('should apply disabled class and attribute when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--disabled')).toBe(true);
    expect(button.disabled).toBe(true);
  });

  it('should show icon at start position by default', () => {
    fixture.componentRef.setInput('icon', 'add');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    const icon = button.querySelector('.material-icons');
    const content = button.querySelector('.button__content');

    expect(button.firstElementChild).toBe(icon);
  });

  it('should show icon at end position when specified', () => {
    fixture.componentRef.setInput('icon', 'arrow_forward');
    fixture.componentRef.setInput('iconPosition', 'end');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('button--icon-end')).toBe(true);
  });
});
