import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  readonly username = signal('');
  readonly password = signal('');
  readonly confirmPassword = signal('');
  readonly isLoading = signal(false);
  readonly errorMessage = signal('');
  readonly isSignUpMode = signal(false);
  readonly allowRegistration = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    void this.authService.getConfig().then((config) => {
      this.allowRegistration.set(config.allowRegistration);
    });
  }

  toggleMode(): void {
    this.isSignUpMode.update((v) => !v);
    this.errorMessage.set('');
    this.confirmPassword.set('');
  }

  async onSubmit(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set('');

    if (this.isSignUpMode()) {
      await this.handleSignUp();
    } else {
      await this.handleLogin();
    }

    this.isLoading.set(false);
  }

  private async handleLogin(): Promise<void> {
    const success = await this.authService.login(this.username(), this.password());
    if (success) {
      void this.router.navigate(['/']);
    } else {
      this.errorMessage.set('Invalid username or password.');
    }
  }

  private async handleSignUp(): Promise<void> {
    if (this.password() !== this.confirmPassword()) {
      this.errorMessage.set('Passwords do not match.');
      this.isLoading.set(false);
      return;
    }

    const result = await this.authService.register(this.username(), this.password());
    if (result.success) {
      void this.router.navigate(['/']);
    } else {
      this.errorMessage.set(result.error ?? 'Registration failed.');
    }
  }
}
