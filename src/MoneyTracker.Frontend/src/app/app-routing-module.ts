import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ForecastsPageComponent } from './pages/forecasts-page.component';
import { HomePageComponent } from './pages/home-page.component';
import { LoginPageComponent } from './pages/login-page.component';
import { PaymentsPageComponent } from './pages/payments-page.component';
import { authGuard } from './auth/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: '', component: HomePageComponent, canActivate: [authGuard] },
  { path: 'payments', component: PaymentsPageComponent, canActivate: [authGuard] },
  { path: 'forecasts', component: ForecastsPageComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
