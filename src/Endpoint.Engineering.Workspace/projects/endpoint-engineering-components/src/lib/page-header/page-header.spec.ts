import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PageHeader } from './page-header';

describe('PageHeader', () => {
  let component: PageHeader;
  let fixture: ComponentFixture<PageHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageHeader],
    }).compileComponents();

    fixture = TestBed.createComponent(PageHeader);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'Test Title');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the title', () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector(
      '.page-header__title'
    );
    expect(titleElement.textContent.trim()).toBe('Test Title');
  });

  it('should show back button by default', () => {
    fixture.detectChanges();
    const backButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="arrow_back"]'
    );
    expect(backButton).toBeTruthy();
  });

  it('should hide back button when showBackButton is false', () => {
    fixture.componentRef.setInput('showBackButton', false);
    fixture.detectChanges();

    const backButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="arrow_back"]'
    );
    expect(backButton).toBeFalsy();
  });

  it('should show help button by default', () => {
    fixture.detectChanges();
    const helpButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="help_outline"]'
    );
    expect(helpButton).toBeTruthy();
  });

  it('should hide help button when showHelpButton is false', () => {
    fixture.componentRef.setInput('showHelpButton', false);
    fixture.detectChanges();

    const helpButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="help_outline"]'
    );
    expect(helpButton).toBeFalsy();
  });

  it('should show menu button by default', () => {
    fixture.detectChanges();
    const menuButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="more_vert"]'
    );
    expect(menuButton).toBeTruthy();
  });

  it('should hide menu button when showMenuButton is false', () => {
    fixture.componentRef.setInput('showMenuButton', false);
    fixture.detectChanges();

    const menuButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="more_vert"]'
    );
    expect(menuButton).toBeFalsy();
  });

  it('should emit backClicked event when back button is clicked', () => {
    fixture.detectChanges();
    const backSpy = vi.fn();
    component.backClicked.subscribe(backSpy);

    const backButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="arrow_back"]'
    );
    backButton.dispatchEvent(new CustomEvent('clicked'));

    expect(backSpy).toHaveBeenCalled();
  });

  it('should emit helpClicked event when help button is clicked', () => {
    fixture.detectChanges();
    const helpSpy = vi.fn();
    component.helpClicked.subscribe(helpSpy);

    const helpButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="help_outline"]'
    );
    helpButton.dispatchEvent(new CustomEvent('clicked'));

    expect(helpSpy).toHaveBeenCalled();
  });

  it('should emit menuClicked event when menu button is clicked', () => {
    fixture.detectChanges();
    const menuSpy = vi.fn();
    component.menuClicked.subscribe(menuSpy);

    const menuButton = fixture.nativeElement.querySelector(
      'ee-icon-button[icon="more_vert"]'
    );
    menuButton.dispatchEvent(new CustomEvent('clicked'));

    expect(menuSpy).toHaveBeenCalled();
  });
});
