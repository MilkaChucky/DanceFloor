import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'
import { BaseService } from './base-service';
import { Observable } from "rxjs";
import { Credentials } from '../models/credentials';
import { catchError } from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {

  constructor(private http: HttpClient) {
    super();
  }

  public login(credentials: Credentials): Observable<any> {
    return this.http.post(`${this.backendUrl}/login`, credentials)
      .pipe(
        catchError(this.handleError())
      );
  }

  public logout(): Observable<void> {
    return this.http.post(`${this.backendUrl}/logout`, { })
      .pipe(
        catchError(this.handleError())
      );
  }

  public register(credentials: Credentials): Observable<any> {
    return this.http.post(`${this.backendUrl}/register`, credentials)
      .pipe(
        catchError(this.handleError())
      );
  }
}
