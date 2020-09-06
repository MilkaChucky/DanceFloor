import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'DanceFloor';

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) { }

  get loggedIn(): boolean {
    return this.authService.loggedIn;
  }

  logout(): void {
    this.authService.logout()
      .subscribe(async () => {
        await this.router.navigate(['/']);
      });
  }
}
