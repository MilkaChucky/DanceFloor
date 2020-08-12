import { Component, OnInit } from '@angular/core';
import { AuthService } from "../../services/auth.service";
import { Credentials } from "../../models/credentials";
import { Router } from "@angular/router";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  credentials: Credentials;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.credentials = {
      email: '',
      password: ''
    }
  }

  login() {
    this.authService.login(this.credentials)
      .subscribe(async result => {
        await this.router.navigate(["/"]);
      });
  }
}
