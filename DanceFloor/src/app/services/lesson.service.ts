import { Injectable } from '@angular/core';
import { BaseService } from './base-service';
import { HttpClient } from '@angular/common/http';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { Observable } from 'rxjs';
import { catchError, delay, retryWhen, tap } from 'rxjs/operators';
import { DanceLesson } from '../models/dance-lesson';
import { BallroomDanceLesson } from '../models/ballroom-dance-lesson';
import { ApplicationForLesson } from '../models/application-for-lesson';

const RECONNECT_INTERVAL = 1000;

@Injectable({
  providedIn: 'root'
})
export class LessonService extends BaseService {
  private readonly socket$: WebSocketSubject<ApplicationForLesson> = webSocket({
    url: `${this.backendUrl}/updates/lessons`
  });

  public readonly applications$: Observable<ApplicationForLesson> = this.socket$
    .pipe();

  constructor(private readonly http: HttpClient) {
    super();
  }

  public getLessons(danceHallId: string): Observable<(DanceLesson | BallroomDanceLesson)[]> {
    return this.http.get<(DanceLesson | BallroomDanceLesson)[]>(`${this.backendUrl}/lessons`, { params: { danceHall: danceHallId } })
      .pipe(
        catchError(this.handleError())
      );
  }

  public applyForLesson(application: ApplicationForLesson): void {
    this.socket$.next(application);
  }
}
