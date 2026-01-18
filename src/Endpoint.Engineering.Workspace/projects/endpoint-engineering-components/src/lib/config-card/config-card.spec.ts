import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfigCard } from './config-card';

describe('ConfigCard', () => {
  let component: ConfigCard;
  let fixture: ComponentFixture<ConfigCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigCard],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigCard);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('name', 'E-Commerce API');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the name', () => {
    fixture.detectChanges();
    const nameElement = fixture.nativeElement.querySelector(
      '.config-card__name'
    );
    expect(nameElement.textContent.trim()).toBe('E-Commerce API');
  });

  it('should display the description when provided', () => {
    fixture.componentRef.setInput(
      'description',
      'Complete e-commerce solution'
    );
    fixture.detectChanges();

    const descriptionElement = fixture.nativeElement.querySelector(
      '.config-card__description'
    );
    expect(descriptionElement.textContent.trim()).toBe(
      'Complete e-commerce solution'
    );
  });

  it('should display the default icon', () => {
    fixture.detectChanges();
    const iconElement = fixture.nativeElement.querySelector(
      '.config-card__icon .material-icons'
    );
    expect(iconElement.textContent.trim()).toBe('layers');
  });

  it('should display custom icon', () => {
    fixture.componentRef.setInput('icon', 'code');
    fixture.detectChanges();

    const iconElement = fixture.nativeElement.querySelector(
      '.config-card__icon .material-icons'
    );
    expect(iconElement.textContent.trim()).toBe('code');
  });

  it('should display last used time when provided', () => {
    fixture.componentRef.setInput('lastUsed', 'Used 2 hours ago');
    fixture.detectChanges();

    const lastUsedElement = fixture.nativeElement.querySelector(
      '.config-card__last-used'
    );
    expect(lastUsedElement.textContent.trim()).toContain('Used 2 hours ago');
  });

  it('should render meta items', () => {
    fixture.componentRef.setInput('meta', [
      { icon: 'storage', label: '3 repositories' },
      { icon: 'folder', label: '12 components' },
    ]);
    fixture.detectChanges();

    const metaItems = fixture.nativeElement.querySelectorAll(
      '.config-card__meta-item'
    );
    expect(metaItems.length).toBe(2);
  });

  it('should render tags', () => {
    fixture.componentRef.setInput('tags', [
      { label: 'GitHub', variant: 'github' },
      { label: '.NET 8' },
    ]);
    fixture.detectChanges();

    const tags = fixture.nativeElement.querySelectorAll('ee-tag');
    expect(tags.length).toBe(2);
  });

  it('should emit cardClicked when card is clicked', () => {
    fixture.detectChanges();
    const clickSpy = vi.fn();
    component.cardClicked.subscribe(clickSpy);

    const card = fixture.nativeElement.querySelector('.config-card');
    card.click();

    expect(clickSpy).toHaveBeenCalled();
  });

  it('should emit menuClicked when menu button is clicked', () => {
    fixture.detectChanges();
    const menuSpy = vi.fn();
    component.menuClicked.subscribe(menuSpy);

    const menuButton = fixture.nativeElement.querySelector(
      '.config-card__menu'
    );
    menuButton.click();

    expect(menuSpy).toHaveBeenCalled();
  });
});
