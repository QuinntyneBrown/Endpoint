import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SearchBox } from './search-box';

describe('SearchBox', () => {
  let component: SearchBox;
  let fixture: ComponentFixture<SearchBox>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchBox],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchBox);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the search icon', () => {
    fixture.detectChanges();
    const iconElement = fixture.nativeElement.querySelector(
      '.search-box__icon'
    );
    expect(iconElement.textContent.trim()).toBe('search');
  });

  it('should display the placeholder', () => {
    fixture.componentRef.setInput('placeholder', 'Search configurations...');
    fixture.detectChanges();

    const inputElement = fixture.nativeElement.querySelector(
      '.search-box__input'
    );
    expect(inputElement.placeholder).toBe('Search configurations...');
  });

  it('should emit searchChanged event on input', () => {
    fixture.detectChanges();
    const searchSpy = vi.fn();
    component.searchChanged.subscribe(searchSpy);

    const inputElement = fixture.nativeElement.querySelector(
      '.search-box__input'
    );
    inputElement.value = 'test';
    inputElement.dispatchEvent(new Event('input'));

    expect(searchSpy).toHaveBeenCalledWith('test');
  });

  it('should not show clear button when value is empty', () => {
    fixture.detectChanges();
    const clearButton = fixture.nativeElement.querySelector(
      '.search-box__clear'
    );
    expect(clearButton).toBeFalsy();
  });

  it('should show clear button when value is not empty', () => {
    component.value.set('test');
    fixture.detectChanges();

    const clearButton = fixture.nativeElement.querySelector(
      '.search-box__clear'
    );
    expect(clearButton).toBeTruthy();
  });

  it('should clear value and emit event when clear button is clicked', () => {
    component.value.set('test');
    fixture.detectChanges();

    const searchSpy = vi.fn();
    component.searchChanged.subscribe(searchSpy);

    const clearButton = fixture.nativeElement.querySelector(
      '.search-box__clear'
    );
    clearButton.click();

    expect(component.value()).toBe('');
    expect(searchSpy).toHaveBeenCalledWith('');
  });
});
