import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ForecastFacadeService } from './forecast-facade.service';
import { PaymentFacadeService } from '../payments/payment-facade.service';

type ForecastPageTab = 'expenses' | 'incomes';

@Component({
  selector: 'app-forecasts-page',
  templateUrl: './forecasts-page.component.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForecastsPageComponent {
  activeTab: ForecastPageTab = 'expenses';

  constructor(
    public readonly forecastFacade: ForecastFacadeService,
    public readonly paymentFacade: PaymentFacadeService
  ) {}

  setActiveTab(tab: ForecastPageTab): void {
    this.activeTab = tab;
  }

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }
}
