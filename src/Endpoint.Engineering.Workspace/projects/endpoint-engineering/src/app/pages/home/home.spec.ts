import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HomePage } from './home';

describe('HomePage', () => {
  let component: HomePage;
  let fixture: ComponentFixture<HomePage>;
  let router: Router;

  beforeEach(async () => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [HomePage],
      providers: [
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomePage);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display empty state with title', () => {
    const compiled = fixture.nativeElement;
    const emptyState = compiled.querySelector('ee-empty-state');
    expect(emptyState).toBeTruthy();
  });

  it('should navigate to create page on new composition', () => {
    component.onNewComposition();
    expect(router.navigate).toHaveBeenCalledWith(['/request/create']);
  });

  it('should navigate to requests list on load configuration', () => {
    component.onLoadConfiguration();
    expect(router.navigate).toHaveBeenCalledWith(['/requests']);
  });

  it('should navigate to create with query params on quick option', () => {
    component.onQuickOption('github');
    expect(router.navigate).toHaveBeenCalledWith(
      ['/request/create'],
      { queryParams: { source: 'github' } }
    );
  });

  it('should have quick start section', () => {
    const compiled = fixture.nativeElement;
    const quickStart = compiled.querySelector('.quick-start');
    expect(quickStart).toBeTruthy();
  });

  it('should render quick option components', () => {
    const compiled = fixture.nativeElement;
    const quickOptions = compiled.querySelectorAll('ee-quick-option');
    expect(quickOptions.length).toBe(4);
  });
});
