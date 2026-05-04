import { Component } from '@angular/core';
import { App } from '../app';

type ForecastPageTab = 'expenses' | 'incomes';

@Component({
  selector: 'app-forecasts-page',
  templateUrl: './forecasts-page.component.html',
  standalone: false,
})
export class ForecastsPageComponent {
  activeTab: ForecastPageTab = 'expenses';

  constructor(public readonly app: App) {}

  setActiveTab(tab: ForecastPageTab): void {
    this.activeTab = tab;
  }
}
