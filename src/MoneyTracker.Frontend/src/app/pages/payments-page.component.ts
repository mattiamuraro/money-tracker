import { Component } from '@angular/core';
import { App } from '../app';

type PaymentsPageTab = 'payments' | 'incomes';

interface PaymentCategoryBreakdownItem {
  category: string;
  amount: number;
  count: number;
  percentage: number;
  forecastAmount?: number;
}

@Component({
  selector: 'app-payments-page',
  templateUrl: './payments-page.component.html',
  standalone: false,
})
export class PaymentsPageComponent {
  activeTab: PaymentsPageTab = 'payments';
  isPaymentBreakdownCollapsed = true;

  constructor(public readonly app: App) {}

  setActiveTab(tab: PaymentsPageTab): void {
    this.activeTab = tab;
  }

  togglePaymentBreakdown(): void {
    this.isPaymentBreakdownCollapsed = !this.isPaymentBreakdownCollapsed;
  }

  get selectedPaymentMonthLabel(): string {
    const monthValue = this.app.selectedPaymentMonth;
    const parsed = new Date(`${monthValue}-01T00:00:00`);

    if (Number.isNaN(parsed.getTime())) {
      return monthValue;
    }

    return new Intl.DateTimeFormat(undefined, { month: 'long', year: 'numeric' }).format(parsed);
  }

  get monthIncomeTotal(): number {
    return this.app.incomeTotal;
  }

  get monthPaymentTotal(): number {
    return this.app.paymentTotal;
  }

  get monthNetTotal(): number {
    return this.monthIncomeTotal - this.monthPaymentTotal;
  }

  get showPaymentForecastColumn(): boolean {
    return this.isCurrentPaymentMonth;
  }

  get totalPaymentForecast(): number {
    if (!this.showPaymentForecastColumn) {
      return 0;
    }

    return this.paymentCategoryBreakdown.reduce((total, item) => total + Number(item.forecastAmount ?? 0), 0);
  }

  get paymentCategoryBreakdown(): PaymentCategoryBreakdownItem[] {
    const total = this.monthPaymentTotal;
    const grouped = new Map<string, { amount: number; count: number; oneShotAmount: number; recurringAmount: number }>();

    for (const payment of this.app.payments) {
      const category = payment.category || 'Uncategorized';
      const current = grouped.get(category) ?? { amount: 0, count: 0, oneShotAmount: 0, recurringAmount: 0 };
      const amount = Number(payment.amount ?? 0);

      grouped.set(category, {
        amount: current.amount + amount,
        count: current.count + 1,
        oneShotAmount: current.oneShotAmount + (payment.isOneShot ? amount : 0),
        recurringAmount: current.recurringAmount + (payment.isOneShot ? 0 : amount),
      });
    }

    const today = new Date();
    const currentDayOfMonth = today.getDate();
    const daysInCurrentMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0).getDate();

    return Array.from(grouped.entries())
      .map(([category, values]) => ({
        category,
        amount: values.amount,
        count: values.count,
        percentage: total > 0 ? (values.amount / total) * 100 : 0,
        forecastAmount: this.isCurrentPaymentMonth
          ? values.oneShotAmount + (values.recurringAmount / currentDayOfMonth) * daysInCurrentMonth
          : undefined,
      }))
      .sort((left, right) => right.amount - left.amount);
  }

  private get isCurrentPaymentMonth(): boolean {
    const selectedMonth = this.app.selectedPaymentMonth;
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    return selectedMonth === currentMonth;
  }
}
