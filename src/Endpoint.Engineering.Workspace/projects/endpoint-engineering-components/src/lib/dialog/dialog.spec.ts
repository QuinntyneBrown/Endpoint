import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dialog } from './dialog';

describe('Dialog', () => {
  let component: Dialog;
  let fixture: ComponentFixture<Dialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dialog],
    }).compileComponents();

    fixture = TestBed.createComponent(Dialog);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'Test Dialog');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the title', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('.dialog__title');
    expect(titleElement.textContent.trim()).toBe('Test Dialog');
  });

  it('should show dialog when open is true', () => {
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();

    const dialogOverlay =
      fixture.nativeElement.querySelector('.dialog-overlay');
    expect(dialogOverlay).toBeTruthy();
  });

  it('should hide dialog when open is false', () => {
    fixture.componentRef.setInput('open', false);
    fixture.detectChanges();

    const dialogOverlay =
      fixture.nativeElement.querySelector('.dialog-overlay');
    expect(dialogOverlay).toBeFalsy();
  });

  it('should display icon when provided', () => {
    fixture.componentRef.setInput('icon', 'save');
    fixture.detectChanges();

    const iconElement = fixture.nativeElement.querySelector(
      '.dialog__title-icon'
    );
    expect(iconElement.textContent.trim()).toBe('save');
  });

  it('should show close button by default', () => {
    fixture.detectChanges();
    const closeButton = fixture.nativeElement.querySelector('ee-icon-button');
    expect(closeButton).toBeTruthy();
  });

  it('should hide close button when showCloseButton is false', () => {
    fixture.componentRef.setInput('showCloseButton', false);
    fixture.detectChanges();

    const closeButton = fixture.nativeElement.querySelector('ee-icon-button');
    expect(closeButton).toBeFalsy();
  });

  it('should emit closed event when close button is clicked', () => {
    fixture.detectChanges();
    const closeSpy = vi.fn();
    component.closed.subscribe(closeSpy);

    const closeButton = fixture.nativeElement.querySelector('ee-icon-button');
    closeButton.dispatchEvent(new CustomEvent('clicked'));

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should emit closed event when overlay is clicked', () => {
    fixture.detectChanges();
    const closeSpy = vi.fn();
    component.closed.subscribe(closeSpy);

    const overlay = fixture.nativeElement.querySelector('.dialog-overlay');
    overlay.click();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should not emit closed when dialog content is clicked', () => {
    fixture.detectChanges();
    const closeSpy = vi.fn();
    component.closed.subscribe(closeSpy);

    const dialog = fixture.nativeElement.querySelector('.dialog');
    dialog.click();

    expect(closeSpy).not.toHaveBeenCalled();
  });
});
