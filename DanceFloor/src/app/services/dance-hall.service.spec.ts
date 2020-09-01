import { TestBed } from '@angular/core/testing';

import { DanceHallService } from './dance-hall.service';

describe('DanceHallService', () => {
  let service: DanceHallService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DanceHallService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
