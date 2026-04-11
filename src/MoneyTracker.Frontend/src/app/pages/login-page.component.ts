import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  standalone: false,
})
export class LoginPageComponent implements OnInit {
  username = '';
  password = '';
  confirmPassword = '';
  isLoading = false;
  errorMessage = '';
  isSignUpMode = false;
  allowRegistration = false;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    const config = await this.authService.getConfig();
    this.allowRegistration = config.allowRegistration;
  }

  toggleMode(): void {
    this.isSignUpMode = !this.isSignUpMode;
    this.errorMessage = '';
    this.confirmPassword = '';
  }

  async onSubmit(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';

    if (this.isSignUpMode) {
      await this.handleSignUp();
    } else {
      await this.handleLogin();
    }

    this.isLoading = false;
  }

  private async handleLogin(): Promise<void> {
    const success = await this.authService.login(this.username, this.password);
    if (success) {
      this.router.navigate(['/']);
    } else {
      this.errorMessage = 'Invalid username or password.';
    }
  }

  private async handleSignUp(): Promise<void> {
    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      this.isLoading = false;
      return;
    }

    const result = await this.authService.register(this.username, this.password);
    if (result.success) {
      this.router.navigate(['/']);
    } else {
      this.errorMessage = result.error ?? 'Registration failed.';
    }
  }
}
