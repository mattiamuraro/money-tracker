import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ForecastsPageComponent } from './pages/forecasts-page.component';
import { HomePageComponent } from './pages/home-page.component';
import { PaymentsPageComponent } from './pages/payments-page.component';

const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'payments', component: PaymentsPageComponent },
  { path: 'forecasts', component: ForecastsPageComponent },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
