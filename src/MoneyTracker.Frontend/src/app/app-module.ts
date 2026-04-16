import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { AuthInterceptor } from './auth/auth.interceptor';
import { AdminCategoriesPageComponent } from './pages/admin-categories-page.component';
import { ForecastsPageComponent } from './pages/forecasts-page.component';
import { HomePageComponent } from './pages/home-page.component';
import { LoginPageComponent } from './pages/login-page.component';
import { PaymentsPageComponent } from './pages/payments-page.component';
import { ConfirmationDialogComponent } from './components/confirmation-dialog.component';

@NgModule({
  declarations: [
    App,
    HomePageComponent,
    PaymentsPageComponent,
    ForecastsPageComponent,
    LoginPageComponent,
    AdminCategoriesPageComponent,
    ConfirmationDialogComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    CommonModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection(),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  bootstrap: [App]
})
export class AppModule { }
