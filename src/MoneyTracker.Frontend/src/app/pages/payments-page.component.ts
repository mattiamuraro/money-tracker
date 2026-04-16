import { Component } from '@angular/core';
import { App } from '../app';
import { ForecastOccurrenceRow, IncomeRow, PaymentRow } from '../models';

type PaymentsPageTab = 'payments' | 'incomes';

interface PaymentCategoryBreakdownItem {
  category: string;
  amount: number;
  count: number;
  percentage: number;
  forecastAmount?: number;
}

interface PaymentListItem {
  id: string;
  kind: 'payment' | 'forecast';
  description: string;
  category: string;
  date: string;
  amount: number;
  isFutureDated: boolean;
  isForecastLinked: boolean;
  payment?: PaymentRow;
  occurrence?: ForecastOccurrenceRow;
}

interface IncomeListItem {
  id: string;
  kind: 'income' | 'forecast';
  description: string;
  date: string;
  amount: number;
  isFutureDated: boolean;
  isForecastLinked: boolean;
  income?: IncomeRow;
  occurrence?: ForecastOccurrenceRow;
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

  isFutureDated(dateValue: string): boolean {
    const parsed = new Date(dateValue);
    if (Number.isNaN(parsed.getTime())) {
      return false;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    parsed.setHours(0, 0, 0, 0);

    return parsed > today;
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

  get paymentListItems(): PaymentListItem[] {
    const paymentItems: PaymentListItem[] = this.app.payments.map((payment) => ({
      id: `payment-${payment.id}`,
      kind: 'payment',
      description: payment.description,
      category: payment.category || 'Uncategorized',
      date: payment.date,
      amount: Number(payment.amount ?? 0),
      isFutureDated: this.isFutureDated(payment.date),
      isForecastLinked: Boolean(payment.forecastOccurrenceId),
      payment,
    }));

    const pendingForecastItems: PaymentListItem[] = this.filteredPaymentOccurrences.map((occurrence) => ({
      id: `forecast-${occurrence.id}`,
      kind: 'forecast',
      description: occurrence.description,
      category: occurrence.category || 'Uncategorized',
      date: occurrence.expectedDate,
      amount: Number(occurrence.amount ?? 0),
      isFutureDated: this.isFutureDated(occurrence.expectedDate),
      isForecastLinked: true,
      occurrence,
    }));

    return [...paymentItems, ...pendingForecastItems].sort((left, right) => {
      const dateComparison = left.date.localeCompare(right.date);
      if (dateComparison !== 0) {
        return dateComparison;
      }

      if (left.kind !== right.kind) {
        return left.kind === 'forecast' ? -1 : 1;
      }

      return left.description.localeCompare(right.description);
    });
  }

  get incomeListItems(): IncomeListItem[] {
    const incomeItems: IncomeListItem[] = this.app.incomes.map((income) => ({
      id: `income-${income.id}`,
      kind: 'income',
      description: income.description,
      date: income.date,
      amount: Number(income.amount ?? 0),
      isFutureDated: this.isFutureDated(income.date),
      isForecastLinked: Boolean(income.forecastOccurrenceId),
      income,
    }));

    const pendingForecastItems: IncomeListItem[] = this.filteredIncomeOccurrences.map((occurrence) => ({
      id: `income-forecast-${occurrence.id}`,
      kind: 'forecast',
      description: occurrence.description,
      date: occurrence.expectedDate,
      amount: Number(occurrence.amount ?? 0),
      isFutureDated: this.isFutureDated(occurrence.expectedDate),
      isForecastLinked: true,
      occurrence,
    }));

    return [...incomeItems, ...pendingForecastItems].sort((left, right) => {
      const dateComparison = left.date.localeCompare(right.date);
      if (dateComparison !== 0) {
        return dateComparison;
      }

      if (left.kind !== right.kind) {
        return left.kind === 'forecast' ? -1 : 1;
      }

      return left.description.localeCompare(right.description);
    });
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
    const grouped = new Map<string, { amount: number; count: number; oneShotAmount: number; recurringAmount: number; pendingAmount: number }>();

    for (const payment of this.app.payments) {
      const category = payment.category || 'Uncategorized';
      const current = grouped.get(category) ?? { amount: 0, count: 0, oneShotAmount: 0, recurringAmount: 0, pendingAmount: 0 };
      const amount = Number(payment.amount ?? 0);

      grouped.set(category, {
        amount: current.amount + amount,
        count: current.count + 1,
        oneShotAmount: current.oneShotAmount + (payment.isOneShot ? amount : 0),
        recurringAmount: current.recurringAmount + (payment.isOneShot ? 0 : amount),
        pendingAmount: current.pendingAmount,
      });
    }

    for (const occurrence of this.filteredPaymentOccurrences) {
      const category = occurrence.category || 'Uncategorized';
      const current = grouped.get(category) ?? { amount: 0, count: 0, oneShotAmount: 0, recurringAmount: 0, pendingAmount: 0 };
      const amount = Number(occurrence.amount ?? 0);

      grouped.set(category, {
        amount: current.amount + amount,
        count: current.count + 1,
        oneShotAmount: current.oneShotAmount,
        recurringAmount: current.recurringAmount,
        pendingAmount: current.pendingAmount + amount,
      });
    }

    const total = Array.from(grouped.values()).reduce((sum, item) => sum + item.amount, 0);
    const today = new Date();
    const currentDayOfMonth = Math.max(today.getDate(), 1);
    const daysInCurrentMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0).getDate();

    return Array.from(grouped.entries())
      .map(([category, values]) => ({
        category,
        amount: values.amount,
        count: values.count,
        percentage: total > 0 ? (values.amount / total) * 100 : 0,
        forecastAmount: this.isCurrentPaymentMonth
          ? values.pendingAmount + values.oneShotAmount + (values.recurringAmount / currentDayOfMonth) * daysInCurrentMonth
          : undefined,
      }))
      .sort((left, right) => right.amount - left.amount);
  }

  isPendingForecastExpense(item: PaymentListItem): boolean {
    return item.kind === 'forecast';
  }

  isPendingForecastIncome(item: IncomeListItem): boolean {
    return item.kind === 'forecast';
  }

  usePendingForecastExpense(item: PaymentListItem): void {
    if (item.occurrence) {
      this.app.startPaymentFromOccurrence(item.occurrence);
    }
  }

  discardPendingForecastExpense(item: PaymentListItem): void {
    if (item.occurrence) {
      void this.app.discardPendingForecastOccurrence(item.occurrence, false);
    }
  }

  usePendingForecastIncome(item: IncomeListItem): void {
    if (item.occurrence) {
      this.app.startIncomeFromOccurrence(item.occurrence);
    }
  }

  discardPendingForecastIncome(item: IncomeListItem): void {
    if (item.occurrence) {
      void this.app.discardPendingForecastOccurrence(item.occurrence, true);
    }
  }

  trackByPaymentListItem(_: number, item: PaymentListItem): string {
    return item.id;
  }

  trackByIncomeListItem(_: number, item: IncomeListItem): string {
    return item.id;
  }

  private get filteredPaymentOccurrences(): ForecastOccurrenceRow[] {
    const descriptionFilter = (this.app.paymentFilters.descriptionFilter || '').trim().toLocaleLowerCase();

    return this.app.paymentOccurrences.filter((occurrence) => {
      if (this.app.paymentFilters.categoryId && occurrence.paymentCategoryId !== this.app.paymentFilters.categoryId) {
        return false;
      }

      if (descriptionFilter && !occurrence.description.toLocaleLowerCase().includes(descriptionFilter)) {
        return false;
      }

      const amount = Number(occurrence.amount ?? 0);

      if (this.app.paymentFilters.minAmount !== null && amount < this.app.paymentFilters.minAmount) {
        return false;
      }

      if (this.app.paymentFilters.maxAmount !== null && amount > this.app.paymentFilters.maxAmount) {
        return false;
      }

      return true;
    });
  }

  private get filteredIncomeOccurrences(): ForecastOccurrenceRow[] {
    const descriptionFilter = (this.app.incomeFilters.descriptionFilter || '').trim().toLocaleLowerCase();

    return this.app.incomeOccurrences.filter((occurrence) => {
      if (descriptionFilter && !occurrence.description.toLocaleLowerCase().includes(descriptionFilter)) {
        return false;
      }

      const amount = Number(occurrence.amount ?? 0);

      if (this.app.incomeFilters.minAmount !== null && amount < this.app.incomeFilters.minAmount) {
        return false;
      }

      if (this.app.incomeFilters.maxAmount !== null && amount > this.app.incomeFilters.maxAmount) {
        return false;
      }

      return true;
    });
  }

  private get isCurrentPaymentMonth(): boolean {
    const selectedMonth = this.app.selectedPaymentMonth;
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    return selectedMonth === currentMonth;
  }
}
