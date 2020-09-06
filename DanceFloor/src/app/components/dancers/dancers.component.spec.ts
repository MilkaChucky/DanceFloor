import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DancersComponent } from './dancers.component';

describe('DancersComponent', () => {
  let component: DancersComponent;
  let fixture: ComponentFixture<DancersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DancersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DancersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
