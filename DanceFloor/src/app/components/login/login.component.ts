import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Credentials } from '../../models/credentials';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  credentials: Credentials;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) { }

  ngOnInit(): void {
    this.credentials = {
      email: '',
      password: ''
    };
  }

  login(): void {
    this.authService.login(this.credentials)
      .subscribe(async () => {
        await this.router.navigate(['/']);
      });
  }

  loginGoogle(): void {
    this.authService.loginGoogle()
      .pipe(
        tap(() => console.log('Logged in'))
      )
      .subscribe(async () => {
        await this.router.navigate(['/']);
      });
  }

  loginFacebook(): void {
    this.authService.loginFacebook()
      .pipe(
        tap(() => console.log('Logged in'))
      )
      .subscribe(async () => {
        await this.router.navigate(['/']);
      });
  }
}
