import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmptyState } from './empty-state';

describe('EmptyState', () => {
  let component: EmptyState;
  let fixture: ComponentFixture<EmptyState>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyState],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyState);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'Test Empty State');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the title', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector(
      '.empty-state__title'
    );
    expect(titleElement.textContent.trim()).toBe('Test Empty State');
  });

  it('should display the description when provided', () => {
    fixture.componentRef.setInput(
      'description',
      'This is a test description.'
    );
    fixture.detectChanges();

    const descriptionElement = fixture.nativeElement.querySelector(
      '.empty-state__description'
    );
    expect(descriptionElement.textContent.trim()).toBe(
      'This is a test description.'
    );
  });

  it('should not display description when not provided', () => {
    fixture.detectChanges();
    const descriptionElement = fixture.nativeElement.querySelector(
      '.empty-state__description'
    );
    expect(descriptionElement).toBeFalsy();
  });

  it('should display the default icon', () => {
    fixture.detectChanges();
    const iconElement = fixture.nativeElement.querySelector(
      '.empty-state__icon .material-icons'
    );
    expect(iconElement.textContent.trim()).toBe('layers');
  });

  it('should display a custom icon when provided', () => {
    fixture.componentRef.setInput('icon', 'folder');
    fixture.detectChanges();

    const iconElement = fixture.nativeElement.querySelector(
      '.empty-state__icon .material-icons'
    );
    expect(iconElement.textContent.trim()).toBe('folder');
  });

  it('should have actions slot', () => {
    fixture.detectChanges();
    const actionsElement = fixture.nativeElement.querySelector(
      '.empty-state__actions'
    );
    expect(actionsElement).toBeTruthy();
  });

  it('should have quick-start slot', () => {
    fixture.detectChanges();
    const quickStartElement = fixture.nativeElement.querySelector(
      '.empty-state__quick-start'
    );
    expect(quickStartElement).toBeTruthy();
  });
});
