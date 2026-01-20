import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminComponents } from './admin-components';

describe('AdminComponents', () => {
  let component: AdminComponents;
  let fixture: ComponentFixture<AdminComponents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminComponents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminComponents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
