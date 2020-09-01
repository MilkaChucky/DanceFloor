import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BaseService } from './base-service';
import { Observable, of, from } from 'rxjs';
import { Credentials } from '../models/credentials';
import { catchError, concatMap, tap } from 'rxjs/operators';
import { FacebookLoginProvider, GoogleLoginProvider, SocialAuthService } from 'angularx-social-login';
import { TokenResponse } from '../models/token-response';

enum ExternalLoginScheme {
  GoogleIdToken= 'IdToken.Google',
  FacebookIdToken = 'IdToken.Facebook'
}

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseService {

  constructor(
    private readonly http: HttpClient,
    private readonly socialAuthService: SocialAuthService) {
    super();
  }

  public login(credentials: Credentials): Observable<void> {
    if (sessionStorage.getItem('token')) {
      return of<void>(null);
    }

    return this.http.post<TokenResponse>(`${this.backendUrl}/login`, credentials)
      .pipe(
        tap(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError())
      );
  }

  public logout(): Observable<void> {
    return this.http.post(`${this.backendUrl}/logout`, { })
      .pipe(
        tap(() => sessionStorage.removeItem('token')),
        concatMap(() => from(this.socialAuthService.signOut())),
        catchError(this.handleError())
      );
  }

  public register(credentials: Credentials): Observable<void> {
    return this.http.post<TokenResponse>(`${this.backendUrl}/register`, credentials)
      .pipe(
        tap(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError())
      );
  }

  private loginExternal(providerId: string, scheme: ExternalLoginScheme): Observable<void> {
    if (sessionStorage.getItem('token')) {
      return of<void>(null);
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
        tap(response => sessionStorage.setItem('token', response.token)),
        catchError(this.handleError())
      );
  }

  public loginGoogle(): Observable<void> {
    return this.loginExternal(GoogleLoginProvider.PROVIDER_ID, ExternalLoginScheme.GoogleIdToken);
  }

  public loginFacebook(): Observable<void> {
    return this.loginExternal(FacebookLoginProvider.PROVIDER_ID, ExternalLoginScheme.FacebookIdToken);
  }
}

