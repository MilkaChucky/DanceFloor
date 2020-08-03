import { Component } from '@angular/core';
import { FacebookLoginProvider, GoogleLoginProvider, SocialAuthService, SocialUser } from "angularx-social-login";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'DanceFloor';
  user: SocialUser;

  constructor(private socialAuthService: SocialAuthService) { }

  signInWithGoogle(): void {
    this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID)
      .then(user => {
        this.user = user;
        console.log(user)
      })
      .catch(error => console.error(error));
  }

  signInWithFacebook(): void {
    this.socialAuthService.signIn(FacebookLoginProvider.PROVIDER_ID)
      .then(user => {
        this.user = user;
        console.log(user)
      })
      .catch(error => console.error(error));
  }

  signOut(): void {
    this.socialAuthService.signOut()
      .then(r => {
        this.user = r;
        console.log(r)
      });
  }
}
