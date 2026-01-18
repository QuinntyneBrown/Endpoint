import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IconButton } from './icon-button';

describe('IconButton', () => {
  let component: IconButton;
  let fixture: ComponentFixture<IconButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconButton],
    }).compileComponents();

    fixture = TestBed.createComponent(IconButton);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'arrow_back');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the icon', () => {
    fixture.detectChanges();
    const iconElement = fixture.nativeElement.querySelector('.material-icons');
    expect(iconElement.textContent.trim()).toBe('arrow_back');
  });

  it('should emit clicked event when clicked', () => {
    const clickSpy = vi.fn();
    component.clicked.subscribe(clickSpy);

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

  it('should apply back variant class', () => {
    fixture.componentRef.setInput('variant', 'back');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('icon-button--back')).toBe(true);
  });

  it('should apply disabled class when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('icon-button--disabled')).toBe(true);
    expect(button.disabled).toBe(true);
  });

  it('should set aria-label from input', () => {
    fixture.componentRef.setInput('ariaLabel', 'Go back');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Go back');
  });

  it('should use icon as aria-label fallback', () => {
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('arrow_back');
  });
});
