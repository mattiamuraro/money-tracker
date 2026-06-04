import { Component, ChangeDetectionStrategy, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ForecastFacadeService } from '../forecasts/forecast-facade.service';
import { PaymentFacadeService } from '../payments/payment-facade.service';
import { HomeDashboardSummaryService } from './home-dashboard-summary.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePageComponent {
  readonly latestPayment = computed(() => this.paymentFacade.payments()[0] ?? null);
  readonly nextUpcomingExpense = computed(() => this.forecastFacade.upcomingExpenses()[0] ?? null);

  readonly effectivePaymentCount = computed(() =>
    this.homeSummary.hasSummary() ? this.homeSummary.summary()!.paymentsCount : this.paymentFacade.payments().length
  );

  readonly effectivePaymentTotal = computed(() =>
    this.homeSummary.hasSummary() ? this.homeSummary.summary()!.paymentTotal : this.paymentFacade.paymentTotal()
  );

  readonly effectiveLatestPaymentDescription = computed(() =>
    this.homeSummary.hasSummary()
      ? this.homeSummary.summary()!.latestPaymentDescription
      : this.latestPayment()?.description ?? null
  );

  readonly effectiveLatestPaymentDate = computed(() =>
    this.homeSummary.hasSummary()
      ? this.homeSummary.summary()!.latestPaymentDate
      : this.latestPayment()?.date ?? null
  );

  readonly effectiveNextUpcomingExpenseDescription = computed(() =>
    this.homeSummary.hasSummary()
      ? this.homeSummary.summary()!.nextUpcomingExpenseDescription
      : this.nextUpcomingExpense()?.description ?? null
  );

  readonly chartTotals = computed(() => {
    if (this.homeSummary.hasSummary()) {
      const summary = this.homeSummary.summary()!;
      const income = summary.forecastIncomeTotal;
      const expense = summary.forecastExpenseTotal;
      const total = income + expense;
      return { income, expense, total };
    }

    const income = this.forecastFacade.incomeTotal();
    const expense = this.forecastFacade.expenseTotal();
    const total = income + expense;
    return { income, expense, total };
  });

  // Computed income chart width
  incomeChartWidth = computed(() => {
    const totals = this.chartTotals();
    return totals.total > 0 ? (totals.income / totals.total) * 100 : 0;
  });

  // Computed expense chart width
  expenseChartWidth = computed(() => {
    const totals = this.chartTotals();
    return totals.total > 0 ? (totals.expense / totals.total) * 100 : 0;
  });

  // Computed one-shot payment ratio
  oneShotPaymentRatio = computed(() => {
    const payments = this.paymentFacade.payments();
    if (!payments.length) return 0;
    const oneShotCount = payments.filter((p) => p.isOneShot).length;
    return (oneShotCount / payments.length) * 100;
  });

  constructor(
    public readonly forecastFacade: ForecastFacadeService,
    public readonly paymentFacade: PaymentFacadeService,
    public readonly homeSummary: HomeDashboardSummaryService
  ) {
    effect(() => {
      const month = this.paymentFacade.selectedMonth();
      void this.homeSummary.loadSummary(month);
    });
  }
}

