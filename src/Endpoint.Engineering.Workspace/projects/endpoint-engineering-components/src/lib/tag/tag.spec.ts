import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Tag } from './tag';

describe('Tag', () => {
  let component: Tag;
  let fixture: ComponentFixture<Tag>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Tag],
    }).compileComponents();

    fixture = TestBed.createComponent(Tag);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('label', 'Test Tag');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the label', () => {
    fixture.detectChanges();
    const tagElement = fixture.nativeElement.querySelector('.tag');
    expect(tagElement.textContent.trim()).toBe('Test Tag');
  });

  it('should apply default variant by default', () => {
    fixture.detectChanges();
    const tagElement = fixture.nativeElement.querySelector('.tag');
    expect(tagElement.classList.contains('tag--github')).toBe(false);
    expect(tagElement.classList.contains('tag--local')).toBe(false);
  });

  it('should apply github variant', () => {
    fixture.componentRef.setInput('variant', 'github');
    fixture.detectChanges();

    const tagElement = fixture.nativeElement.querySelector('.tag');
    expect(tagElement.classList.contains('tag--github')).toBe(true);
  });

  it('should apply local variant', () => {
    fixture.componentRef.setInput('variant', 'local');
    fixture.detectChanges();

    const tagElement = fixture.nativeElement.querySelector('.tag');
    expect(tagElement.classList.contains('tag--local')).toBe(true);
  });

  it('should apply primary variant', () => {
    fixture.componentRef.setInput('variant', 'primary');
    fixture.detectChanges();

    const tagElement = fixture.nativeElement.querySelector('.tag');
    expect(tagElement.classList.contains('tag--primary')).toBe(true);
  });

  it('should not show remove button by default', () => {
    fixture.detectChanges();
    const removeButton = fixture.nativeElement.querySelector('.tag__remove');
    expect(removeButton).toBeFalsy();
  });

  it('should show remove button when removable', () => {
    fixture.componentRef.setInput('removable', true);
    fixture.detectChanges();

    const removeButton = fixture.nativeElement.querySelector('.tag__remove');
    expect(removeButton).toBeTruthy();
  });

  it('should emit removed event when remove button is clicked', () => {
    fixture.componentRef.setInput('removable', true);
    fixture.detectChanges();

    const removeSpy = vi.fn();
    component.removed.subscribe(removeSpy);

    const removeButton = fixture.nativeElement.querySelector('.tag__remove');
    removeButton.click();

    expect(removeSpy).toHaveBeenCalled();
  });
});
