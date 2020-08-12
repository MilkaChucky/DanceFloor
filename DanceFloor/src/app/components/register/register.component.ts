import { Component, OnInit } from '@angular/core';
import { Credentials } from "../../models/credentials";
import { AuthService } from "../../services/auth.service";
import { Router } from "@angular/router";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
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

  register() {
    this.authService.register(this.credentials)
      .subscribe(async result => {
        await this.router.navigate(['/login', this.credentials])
      });
  }
}
