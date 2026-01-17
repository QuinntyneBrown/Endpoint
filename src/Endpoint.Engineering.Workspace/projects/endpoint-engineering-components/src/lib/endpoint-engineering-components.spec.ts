import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EndpointEngineeringComponents } from './endpoint-engineering-components';

describe('EndpointEngineeringComponents', () => {
  let component: EndpointEngineeringComponents;
  let fixture: ComponentFixture<EndpointEngineeringComponents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EndpointEngineeringComponents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EndpointEngineeringComponents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
