import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BaseService } from './base-service';
import { HttpClient } from '@angular/common/http';
import { DanceHall } from '../models/dance-hall';

@Injectable({
  providedIn: 'root'
})
export class DanceHallService extends BaseService {

  constructor(private readonly http: HttpClient) {
    super();
  }

  public danceHalls(): Observable<DanceHall[]> {
    return this.http.get<DanceHall[]>(`${this.backendUrl}/dance_halls`)
      .pipe(
        catchError(this.handleError())
      );
  }
}
