import { Component, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ForecastFacadeService } from '../forecasts/forecast-facade.service';
import { PaymentFacadeService } from '../payments/payment-facade.service';
import { IncomeFacadeService } from '../incomes/income-facade.service';
import { ForecastOccurrenceRow } from '../forecasts/forecast.models';
import { IncomeRow } from '../incomes/income.models';
import { PaymentRow } from '../payments/payment.models';

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
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentsPageComponent {
  readonly activeTab = signal<PaymentsPageTab>('payments');
  readonly isPaymentBreakdownCollapsed = signal(true);

  // Computed values for template
  readonly selectedPaymentMonthLabel = computed(() => {
    const monthValue = this.paymentFacade.selectedMonth();
    const parsed = new Date(`${monthValue}-01T00:00:00`);
    if (Number.isNaN(parsed.getTime())) return monthValue;
    return new Intl.DateTimeFormat(undefined, { month: 'long', year: 'numeric' }).format(parsed);
  });

  readonly monthIncomeTotal = computed(() => {
    const incomeSum = this.incomeFacade.incomes().reduce((total, income) => total + Number(income.amount ?? 0), 0);
    const forecastSum = this.getFilteredIncomeOccurrences().reduce((total, occurrence) => total + Number(occurrence.amount ?? 0), 0);
    return incomeSum + forecastSum;
  });

  readonly monthPaymentTotal = computed(() => {
    const paymentSum = this.paymentFacade.payments().reduce((total, payment) => total + Number(payment.amount ?? 0), 0);
    const forecastSum = this.getFilteredPaymentOccurrences().reduce((total, occurrence) => total + Number(occurrence.amount ?? 0), 0);
    return paymentSum + forecastSum;
  });

  readonly monthNetTotal = computed(() => {
    return this.monthIncomeTotal() - this.monthPaymentTotal();
  });

  readonly paymentListItems = computed(() => {
    const paymentItems: PaymentListItem[] = this.paymentFacade.payments().map((payment) => ({
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

    const pendingForecastItems: PaymentListItem[] = this.getFilteredPaymentOccurrences().map((occurrence) => ({
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
      if (dateComparison !== 0) return dateComparison;
      if (left.kind !== right.kind) return left.kind === 'forecast' ? -1 : 1;
      return left.description.localeCompare(right.description);
    });
  });

  readonly incomeListItems = computed(() => {
    const incomeItems: IncomeListItem[] = this.incomeFacade.incomes().map((income) => ({
      id: `income-${income.id}`,
      kind: 'income',
      description: income.description,
      date: income.date,
      amount: Number(income.amount ?? 0),
      isFutureDated: this.isFutureDated(income.date),
      isForecastLinked: Boolean(income.forecastOccurrenceId),
      income,
    }));

    const pendingForecastItems: IncomeListItem[] = this.getFilteredIncomeOccurrences().map((occurrence) => ({
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
      if (dateComparison !== 0) return dateComparison;
      if (left.kind !== right.kind) return left.kind === 'forecast' ? -1 : 1;
      return left.description.localeCompare(right.description);
    });
  });

  readonly isCurrentPaymentMonth = computed(() => {
    const selectedMonth = this.paymentFacade.selectedMonth();
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    return selectedMonth === currentMonth;
  });

  readonly showPaymentForecastColumn = computed(() => {
    return this.isCurrentPaymentMonth();
  });

  readonly totalPaymentForecast = computed(() => {
    if (!this.showPaymentForecastColumn()) return 0;
    return this.paymentCategoryBreakdown().reduce((total, item) => total + Number(item.forecastAmount ?? 0), 0);
  });

  readonly paymentCategoryBreakdown = computed(() => {
    const grouped = new Map<string, { amount: number; count: number; oneShotAmount: number; recurringAmount: number; pendingAmount: number }>();

    for (const payment of this.paymentFacade.payments()) {
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

    for (const occurrence of this.getFilteredPaymentOccurrences()) {
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
        forecastAmount: this.isCurrentPaymentMonth()
          ? values.pendingAmount + values.oneShotAmount + (values.recurringAmount / currentDayOfMonth) * daysInCurrentMonth
          : undefined,
      }))
      .sort((left, right) => right.amount - left.amount);
  });

  constructor(
    public readonly paymentFacade: PaymentFacadeService,
    public readonly incomeFacade: IncomeFacadeService,
    public readonly forecastFacade: ForecastFacadeService
  ) {}

  setActiveTab(tab: PaymentsPageTab): void {
    this.activeTab.set(tab);
  }

  togglePaymentBreakdown(): void {
    this.isPaymentBreakdownCollapsed.update((v) => !v);
  }

  isFutureDated(dateValue: string): boolean {
    const parsed = new Date(dateValue);
    if (Number.isNaN(parsed.getTime())) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    parsed.setHours(0, 0, 0, 0);
    return parsed > today;
  }

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  trackByCategory(_: number, item: PaymentCategoryBreakdownItem): string {
    return item.category;
  }

  trackByPaymentListItem(_: number, item: PaymentListItem): string {
    return item.id;
  }

  trackByIncomeListItem(_: number, item: IncomeListItem): string {
    return item.id;
  }

  isPendingForecastExpense(item: PaymentListItem): boolean {
    return item.kind === 'forecast';
  }

  isPendingForecastIncome(item: IncomeListItem): boolean {
    return item.kind === 'forecast';
  }

  usePendingForecastExpense(item: PaymentListItem): void {
    if (item.occurrence) {
      this.paymentFacade.startPaymentFromOccurrence(item.occurrence);
    }
  }

  discardPendingForecastExpense(item: PaymentListItem): void {
    if (item.occurrence) {
      void this.forecastFacade.discardForecastOccurrence(item.occurrence, false);
    }
  }

  usePendingForecastIncome(item: IncomeListItem): void {
    if (item.occurrence) {
      this.incomeFacade.startIncomeFromOccurrence(item.occurrence);
    }
  }

  discardPendingForecastIncome(item: IncomeListItem): void {
    if (item.occurrence) {
      void this.forecastFacade.discardForecastOccurrence(item.occurrence, true);
    }
  }

  private getFilteredPaymentOccurrences(): ForecastOccurrenceRow[] {
    return this.paymentFacade.filteredOccurrences();
  }

  private getFilteredIncomeOccurrences(): ForecastOccurrenceRow[] {
    return this.incomeFacade.filteredOccurrences();
  }
}
