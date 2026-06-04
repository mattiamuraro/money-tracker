import { Component, NgZone, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { AuthService } from './auth/auth.service';
import { ConfirmationDialogService } from './shared/services/confirmation-dialog.service';
import { ConfirmationDialogComponent } from './shared/components/confirmation-dialog.component';
import { PaymentFacadeService } from './features/payments/payment-facade.service';
import { IncomeFacadeService } from './features/incomes/income-facade.service';
import { ForecastFacadeService } from './features/forecasts/forecast-facade.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: true,
  styleUrl: './app.css',
  imports: [CommonModule, RouterModule, ConfirmationDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  public isLoginRoute = false;
  public theme = signal<'light' | 'dark'>('dark');
  public isLoading = signal(false);

  private readonly themeStorageKey = 'money-tracker.theme';

  public dialogState: {
    isVisible: boolean;
    options: {
      title?: string;
      message: string;
      confirmButtonText?: string;
      cancelButtonText?: string;
      isDangerous?: boolean;
    };
  } = {
    isVisible: false,
    options: { message: '' },
  };

  constructor(
    private readonly ngZone: NgZone,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly confirmationDialogService: ConfirmationDialogService,
    public readonly paymentFacade: PaymentFacadeService,
    public readonly incomeFacade: IncomeFacadeService,
    public readonly forecastFacade: ForecastFacadeService
  ) {}

  async ngOnInit(): Promise<void> {
    this.confirmationDialogService.getDialogState().subscribe((state) => {
      this.ngZone.run(() => {
        this.dialogState = state;
      });
    });

    this.initializeTheme();
    this.isLoginRoute = this.router.url.startsWith('/login');

    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd)
    ).subscribe((e: NavigationEnd) => {
      this.isLoginRoute = e.urlAfterRedirects.startsWith('/login');

      if (!this.isLoginRoute) {
        void this.loadDataForRoute(e.urlAfterRedirects);
      }
    });

    if (!this.isLoginRoute) {
      await this.loadDataForRoute(this.router.url);
    }
  }

  public get isDarkTheme(): boolean {
    return this.theme() === 'dark';
  }

  public toggleTheme(): void {
    this.setTheme(this.isDarkTheme ? 'light' : 'dark', true);
  }

  public async reloadDashboard(): Promise<void> {
    await this.loadDataForRoute(this.router.url, true);
  }

  public logout(): void {
    this.authService.logout();
  }

  public onDialogConfirmed(): void {
    this.confirmationDialogService.confirm();
  }

  public onDialogCancelled(): void {
    this.confirmationDialogService.cancel();
  }

  private async loadDataForRoute(url: string, force = false): Promise<void> {
    this.isLoading.set(true);
    try {
      if (url.startsWith('/payments')) {
        await Promise.allSettled([
          this.paymentFacade.loadAll(force),
          this.incomeFacade.loadAll(),
        ]);
        return;
      }

      if (url.startsWith('/forecasts')) {
        await Promise.allSettled([
          this.forecastFacade.loadAll(force),
          this.paymentFacade.loadCategoriesOnly(force),
        ]);
        return;
      }

      if (url.startsWith('/admin/categories')) {
        await this.paymentFacade.loadCategoriesOnly(force);
        return;
      }

      await Promise.allSettled([
        this.paymentFacade.loadAll(force),
        this.incomeFacade.loadAll(),
        this.forecastFacade.loadAll(force),
      ]);
    } finally {
      this.isLoading.set(false);
    }
  }

  private initializeTheme(): void {
    const preferredTheme = this.getPreferredTheme();
    this.setTheme(preferredTheme, false);
  }

  private getPreferredTheme(): 'light' | 'dark' {
    try {
      const stored = localStorage.getItem(this.themeStorageKey);
      if (stored === 'light' || stored === 'dark') {
        return stored;
      }
    } catch {
      // ignore
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private setTheme(theme: 'light' | 'dark', persist: boolean): void {
    this.theme.set(theme);
    document.documentElement.setAttribute('data-theme', theme);
    if (persist) {
      try {
        localStorage.setItem(this.themeStorageKey, theme);
      } catch {
        // ignore
      }
    }
  }
}
