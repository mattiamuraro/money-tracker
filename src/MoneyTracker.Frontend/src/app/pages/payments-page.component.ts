import { Component } from '@angular/core';
import { App } from '../app';

type PaymentsPageTab = 'payments' | 'incomes';

@Component({
  selector: 'app-payments-page',
  templateUrl: './payments-page.component.html',
  standalone: false,
})
export class PaymentsPageComponent {
  activeTab: PaymentsPageTab = 'payments';

  constructor(public readonly app: App) {}

  setActiveTab(tab: PaymentsPageTab): void {
    this.activeTab = tab;
  }
}
