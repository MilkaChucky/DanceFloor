import { Injectable } from '@angular/core';
import { BaseService } from './base-service';
import { HttpClient, HttpParams } from '@angular/common/http';
import { UserInfo } from '../models/user-info';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseService {

  constructor(
    private readonly http: HttpClient,
    private readonly snackBar: MatSnackBar
  ) {
    super();
  }

  public getUserInfos(userIds: string[]): Observable<UserInfo[]> {
    const params = new HttpParams({
      fromObject: { userIds }
    });

    return this.http.get<UserInfo[]>(`${this.backendUrl}/userinfo`, { params })
      .pipe(
        catchError(this.handleError(this.snackBar))
      );
  }
}
