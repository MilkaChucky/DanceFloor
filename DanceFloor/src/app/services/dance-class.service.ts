import { Injectable } from '@angular/core';
import { BaseService } from './base-service';
import { HttpClient } from '@angular/common/http';
import { from, Observable, Subject, EMPTY, concat } from 'rxjs';
import { catchError, concatMap, map, retry, tap } from 'rxjs/operators';
import { GroupDanceClass } from '../models/group-dance-class';
import { BallroomDanceClass } from '../models/ballroom-dance-class';
import { HubConnectionBuilder, HubConnectionState } from '@aspnet/signalr';
import { DanceHall, DanceClasses } from '../models/dance-hall';
import { DanceClass } from '../models/dance-class';
import { UserInfo } from '../models/user-info';
import { UserService } from './user.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class DanceClassService extends BaseService {
  // private readonly socket$: WebSocketSubject<(classId: string, userId: string, pairId?: string)> = webSocket({
  //   url: `${this.backendUrl}/updates/dance-classes`
  // });

  // public readonly applications$: Observable<(classId: string, userId: string, pairId?: string)> = this.socket$
  //   .pipe();

  private readonly hubConnection = new HubConnectionBuilder()
    .withUrl(`${this.backendUrl}/updates/classes`, {
      accessTokenFactory: () => sessionStorage.getItem('token')
    })
    .build();

  private classes$: Subject<DanceClasses> = new Subject<DanceClasses>();
  private classes: DanceClasses = [];

  constructor(
    private readonly http: HttpClient,
    private readonly userService: UserService,
    private readonly snackBar: MatSnackBar
  ) {
    super();

    this.hubConnection.on(
      'ReceiveApplication',
      (classId: string, userInfo: UserInfo, pairId?: string) => this.handleApplication(classId, userInfo, pairId)
    );

    this.hubConnection.on(
      'ReceiveCancellation',
      (classId: string, userId: string) => this.handleCancellation(classId, userId)
    );

    this.hubConnection.onclose(error => {
      if (error) {
        console.error(error);
      }
      console.log('Connection closed');
    });
  }

  private handleApplication(classId: string, userInfo: UserInfo, pairId?: string): void {
    for (const danceClass of this.classes) {
      if (danceClass.id === classId) {
        if ('dancers' in danceClass) {
          danceClass.dancers.push(userInfo);
        }
        else if ('pairs' in danceClass) {
          if (pairId) {
            const idx = danceClass.pairs.findIndex(pair => pair.some(dancer => (dancer as UserInfo).id === pairId));

            if (idx !== -1) {
              danceClass.pairs[idx].push(userInfo);
            }
          } else {
            danceClass.pairs.push([ userInfo ]);
          }
        }
      }
    }

    this.classes$.next(this.classes);
  }

  private handleCancellation(classId: string, userId: string): void {
    for (const danceClass of this.classes) {
      if (danceClass.id === classId) {
        if ('dancers' in danceClass) {
          danceClass.dancers = danceClass.dancers.filter(dancer => (dancer as UserInfo).id !== userId);
        }
        else if ('pairs' in danceClass) {
          const idx = danceClass.pairs.findIndex(pair => pair.some(dancer => (dancer as UserInfo).id === userId));

          if (idx !== -1) {
            danceClass.pairs[idx] = danceClass.pairs[idx].filter(dancer => (dancer as UserInfo).id !== userId);

            if (danceClass.pairs[idx].length === 0) {
              danceClass.pairs.splice(idx, 1);
            }
          }
        }
      }
    }

    this.classes$.next(this.classes);
  }

  private openConnection(): Observable<void> {
    if (this.hubConnection.state === HubConnectionState.Connected) {
      return EMPTY;
    }

    return from(this.hubConnection.start())
      .pipe(
        tap(() => console.log('Trying to connect...')),
        retry(10),
        tap(() => console.log('Connection started')),
        catchError(this.handleError(this.snackBar))
      );
  }

  private closeConnection(): Observable<void> {
    if (this.hubConnection.state === HubConnectionState.Disconnected) {
      return EMPTY;
    }

    return from(this.hubConnection.stop())
      .pipe(
        tap(() => console.log('Connection closed')),
        catchError(this.handleError(this.snackBar))
      );
  }

  private danceHallsToDanceClasses(danceHalls: DanceHall[]): DanceClasses {
    return danceHalls
      .map(danceHall =>
        danceHall.classes.map<(DanceClass | GroupDanceClass | BallroomDanceClass)>(danceClass => {
          danceClass.address = danceHall.address;
          danceClass.room = danceHall.room;

          return danceClass;
        })
      )
      .reduce((acc, val) => [...acc, ...val]);
  }

  private lookupUserInfosForDanceClasses(danceClasses: DanceClasses): Observable<DanceClasses> {
    const userIds = danceClasses.map(danceClass => danceClass.teacher as string)
      .concat(danceClasses
        .map(danceClass => {
          if ('dancers' in danceClass) {
            return danceClass.dancers as string[];
          } else if ('pairs' in danceClass) {
            return danceClass.pairs.length > 0 ?
              danceClass.pairs.reduce((acc, val) => [...acc, ...val]) as string[] :
              [] as string[];
          } else {
            return [] as string[];
          }
        })
        .reduce((acc, val) => [...acc, ...val])
      );

    return this.userService.getUserInfos([...new Set(userIds)])
      .pipe(
        map(userInfos => {
          for (const danceClass of danceClasses) {
            danceClass.teacher = userInfos.find(userInfo => userInfo.id === danceClass.teacher);

            if ('dancers' in danceClass) {
              danceClass.dancers = danceClass.dancers.map(dancer => userInfos.find(userInfo => userInfo.id === dancer));
            } else if ('pairs' in danceClass) {
              for (let i = 0; i < danceClass.pairs.length; i++) {
                danceClass.pairs[i] = danceClass.pairs[i].map(dancer => userInfos.find(userInfo => userInfo.id === dancer));
              }
            }
          }

          return danceClasses;
        })
      );
  }

  public getClasses(danceHallId?: string): Observable<DanceClasses> {
    return concat(
      this.http.get<DanceHall[]>(
        `${this.backendUrl}/dance_halls`,
        { params: danceHallId ? { danceHall: danceHallId } : { } })
        .pipe(
          map(danceHalls => this.danceHallsToDanceClasses(danceHalls)),
          concatMap(danceClasses => this.lookupUserInfosForDanceClasses(danceClasses)),
          tap(result => { this.classes = result; })
        ),
      this.openConnection()
        .pipe(
          concatMap(() => this.classes$)
        )
    ).pipe(
      catchError(this.handleError(this.snackBar))
    );
  }

  public applyForClass(classId: string, pairId?: string): Observable<void> {
    return this.http.post<never>(`${this.backendUrl}/classes/${classId}/join`, { }, {
      params: pairId ? { pairId } : { }
    })
      .pipe(
        catchError(this.handleError(this.snackBar))
      );
    // return this.openConnection()
    //   .pipe(
    //     concatMap(() => from(this.hubConnection.invoke('ApplyForClass', classId, pairId))),
    //     catchError(this.handleError())
    //   );
  }

  public cancelApplication(classId: string): Observable<void> {
    return this.http.post<never>(`${this.backendUrl}/classes/${classId}/leave`, { })
      .pipe(
        catchError(this.handleError(this.snackBar))
      );
    // return this.openConnection()
    //   .pipe(
    //     concatMap(() => from(this.hubConnection.invoke('CancelApplication', classId))),
    //     catchError(this.handleError())
    //   );
  }
}
