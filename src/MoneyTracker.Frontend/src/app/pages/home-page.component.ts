import { Component } from '@angular/core';
import { App } from '../app';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  standalone: false,
})
export class HomePageComponent {
  constructor(public readonly app: App) {}

  public get incomeChartWidth(): number {
    const income = this.app.forecastIncomeTotal;
    const expense = this.app.forecastExpenseTotal;
    const total = income + expense;
    return total > 0 ? (income / total) * 100 : 0;
  }

  public get expenseChartWidth(): number {
    const income = this.app.forecastIncomeTotal;
    const expense = this.app.forecastExpenseTotal;
    const total = income + expense;
    return total > 0 ? (expense / total) * 100 : 0;
  }

  public get oneShotPaymentRatio(): number {
    if (!this.app.payments.length) {
      return 0;
    }

    const oneShotCount = this.app.payments.filter((payment) => payment.isOneShot).length;
    return (oneShotCount / this.app.payments.length) * 100;
  }
}
