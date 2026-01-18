import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Badge } from './badge';

describe('Badge', () => {
  let component: Badge;
  let fixture: ComponentFixture<Badge>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Badge],
    }).compileComponents();

    fixture = TestBed.createComponent(Badge);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('label', 'Test Badge');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the label', () => {
    fixture.detectChanges();
    const badgeElement = fixture.nativeElement.querySelector('.badge');
    expect(badgeElement.textContent.trim()).toBe('Test Badge');
  });

  it('should apply pending variant by default', () => {
    fixture.detectChanges();
    const badgeElement = fixture.nativeElement.querySelector('.badge');
    expect(badgeElement.classList.contains('badge--pending')).toBe(true);
  });

  it('should apply success variant', () => {
    fixture.componentRef.setInput('variant', 'success');
    fixture.detectChanges();

    const badgeElement = fixture.nativeElement.querySelector('.badge');
    expect(badgeElement.classList.contains('badge--success')).toBe(true);
  });

  it('should apply processing variant', () => {
    fixture.componentRef.setInput('variant', 'processing');
    fixture.detectChanges();

    const badgeElement = fixture.nativeElement.querySelector('.badge');
    expect(badgeElement.classList.contains('badge--processing')).toBe(true);
  });

  it('should apply error variant', () => {
    fixture.componentRef.setInput('variant', 'error');
    fixture.detectChanges();

    const badgeElement = fixture.nativeElement.querySelector('.badge');
    expect(badgeElement.classList.contains('badge--error')).toBe(true);
  });
});
