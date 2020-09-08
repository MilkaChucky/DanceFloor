import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BaseService } from './base-service';
import { Observable, of, from, concat, EMPTY } from 'rxjs';
import { Credentials } from '../models/credentials';
import { catchError, concatMap, map, tap } from 'rxjs/operators';
import { FacebookLoginProvider, GoogleLoginProvider, SocialAuthService } from 'angularx-social-login';
import { TokenResponse } from '../models/token-response';
import { TokenUserInfo } from '../models/token-user-info';
import { MatSnackBar } from '@angular/material/snack-bar';

enum ExternalLoginScheme {
  GoogleIdToken= 'IdToken.Google',
  FacebookIdToken = 'IdToken.Facebook'
}

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {
  private externalLoggedIn: boolean;

  constructor(
    private readonly http: HttpClient,
    private readonly socialAuthService: SocialAuthService,
    private readonly snackBar: MatSnackBar
  ) {
    super();

    socialAuthService.authState.subscribe(user => {
      this.externalLoggedIn = user !== null;
    });
  }

  public get currentUser(): TokenUserInfo {
    const token = sessionStorage.getItem('token');
    const jwtData = token.split('.')[1];
    const decodedJwtJsonData = window.atob(jwtData);
    const decodedJwtData = JSON.parse(decodedJwtJsonData);

    return {
      id: decodedJwtData.sub,
      name: decodedJwtData.nameid,
      email: decodedJwtData.email
    };
  }

  public get loggedIn(): boolean {
    return sessionStorage.getItem('token') !== null;
  }

  public login(credentials: Credentials): Observable<void> {
    if (sessionStorage.getItem('token')) {
      return EMPTY;
    }

    return this.http.post<TokenResponse>(`${this.backendUrl}/login`, credentials)
      .pipe(
        map(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError(this.snackBar))
      );
  }

  public logout(): Observable<void> {
    if (!sessionStorage.getItem('token')) {
      return EMPTY;
    }

    return this.http.post(`${this.backendUrl}/logout`, { })
      .pipe(
        tap(() => sessionStorage.removeItem('token')),
        concatMap(() => this.externalLoggedIn ? from(this.socialAuthService.signOut()) : EMPTY),
        catchError(this.handleError(this.snackBar))
      );
  }

  public register(credentials: Credentials): Observable<void> {
    return this.http.post<TokenResponse>(`${this.backendUrl}/register`, credentials)
      .pipe(
        map(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError(this.snackBar))
      );
  }

  private loginExternal(providerId: string, scheme: ExternalLoginScheme): Observable<void> {
    if (sessionStorage.getItem('token')) {
      return EMPTY;
    }

    return from(this.socialAuthService.signIn(providerId))
      .pipe(
        tap(user => console.log(user)),
        concatMap(user => {
          const httpHeader = new HttpHeaders({
            ExternalLoginScheme: scheme,
            IdToken: user.idToken
          });

          return this.http.post<TokenResponse>(`${this.backendUrl}/login/external`, { }, { headers: httpHeader });
        }),
        map(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError(this.snackBar))
      );
  }

  public loginGoogle(): Observable<void> {
    return this.loginExternal(GoogleLoginProvider.PROVIDER_ID, ExternalLoginScheme.GoogleIdToken);
  }

  public loginFacebook(): Observable<void> {
    return this.loginExternal(FacebookLoginProvider.PROVIDER_ID, ExternalLoginScheme.FacebookIdToken);
  }
}

