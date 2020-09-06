import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DanceClassesComponent } from './dance-classes.component';

describe('DanceClassesComponent', () => {
  let component: DanceClassesComponent;
  let fixture: ComponentFixture<DanceClassesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DanceClassesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DanceClassesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
