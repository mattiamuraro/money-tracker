import { Injectable, signal, computed } from '@angular/core';
import { MoneyTrackerApiService } from '../../money-tracker-api.service';
import {
  PaymentCategory,
  PaymentFormModel,
  PaymentQuery,
  PaymentRow,
} from './payment.models';
import { ForecastOccurrenceRow, OccurrenceDeleteAction } from '../forecasts/forecast.models';

/**
 * Manages payment and category data state using Angular signals.
 * Responsible for:
 * - Loading and storing payment/category data
 * - API mutations (create, update, delete)
 * - Data filtering and transformations
 */
@Injectable({ providedIn: 'root' })
export class PaymentDataService {
  // Data signals
  readonly categories = signal<PaymentCategory[]>([]);
  readonly payments = signal<PaymentRow[]>([]);
  readonly paymentOccurrences = signal<ForecastOccurrenceRow[]>([]);

  // Query state
  readonly selectedMonth = signal<string>(this.getCurrentMonthInput());
  readonly categoryFilter = signal<string>('');
  readonly descriptionFilter = signal<string>('');
  readonly minAmount = signal<number | null>(null);
  readonly maxAmount = signal<number | null>(null);

  // Computed values
  readonly filteredPayments = computed(() => this.applyPaymentFilters());
  readonly filteredOccurrences = computed(() => this.applyOccurrenceFilters());
  readonly paymentTotal = computed(() =>
    this.payments().reduce((total, p) => total + Number(p.amount ?? 0), 0)
  );
  readonly occurrenceTotal = computed(() =>
    this.filteredOccurrences().reduce((total, o) => total + Number(o.amount ?? 0), 0)
  );
  readonly combinedTotal = computed(() => this.paymentTotal() + this.occurrenceTotal());

  private readonly pageSize = 100;
  private monthReloadTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly monthReloadDelayMs = 250;
  private hasLoadedCategories = false;

  constructor(private readonly apiService: MoneyTrackerApiService) {}

  async loadCategories(force = false): Promise<void> {
    if (!force && this.hasLoadedCategories && this.categories().length > 0) {
      return;
    }

    try {
      const data = await this.apiService.getCategories();
      this.categories.set([...data].sort((a, b) => a.name.localeCompare(b.name)));
      this.hasLoadedCategories = true;
    } catch (error) {
      console.error('Failed to load categories:', error);
      throw error;
    }
  }

  async loadPayments(): Promise<void> {
    try {
      const baseQuery: PaymentQuery = {
        month: this.selectedMonth(),
        pageNumber: 1,
        pageSize: this.pageSize,
        sortBy: 'Date',
        sortOrder: 'asc',
      };

      const firstPage = await this.apiService.getPayments(baseQuery);
      const allItems = [...firstPage.items];
      const totalPages = Math.max(1, Math.ceil(firstPage.totalItems / this.pageSize));

      for (let page = 2; page <= totalPages; page++) {
        const nextPage = await this.apiService.getPayments({ ...baseQuery, pageNumber: page });
        allItems.push(...nextPage.items);
      }

      this.payments.set([...allItems].sort((a, b) => a.date.localeCompare(b.date)));
    } catch (error) {
      console.error('Failed to load payments:', error);
      throw error;
    }
  }

  async loadPaymentOccurrences(): Promise<void> {
    try {
      const occurrences = await this.apiService.getForecastExpenseOccurrences(this.selectedMonth());
      this.paymentOccurrences.set([...occurrences].sort((a, b) => a.expectedDate.localeCompare(b.expectedDate)));
    } catch (error) {
      console.error('Failed to load payment occurrences:', error);
      throw error;
    }
  }

  async createPayment(model: PaymentFormModel): Promise<string> {
    const id = await this.apiService.createPayment(model);
    await Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
    return id;
  }

  async updatePayment(id: string, model: PaymentFormModel): Promise<void> {
    await this.apiService.updatePayment(id, model);
    await Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  async deletePayment(id: string, occurrenceAction?: OccurrenceDeleteAction): Promise<void> {
    await this.apiService.deletePayment(id, occurrenceAction);
    await Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  setSelectedMonth(month: string): void {
    this.selectedMonth.set(month);
    this.scheduleMonthReload();
  }

  setCategoryFilter(categoryId: string): void {
    this.categoryFilter.set(categoryId);
  }

  setDescriptionFilter(description: string): void {
    this.descriptionFilter.set(description);
  }

  setAmountRange(min: number | null, max: number | null): void {
    this.minAmount.set(min);
    this.maxAmount.set(max);
  }

  resetFilters(): void {
    this.selectedMonth.set(this.getCurrentMonthInput());
    this.categoryFilter.set('');
    this.descriptionFilter.set('');
    this.minAmount.set(null);
    this.maxAmount.set(null);
    void Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  private scheduleMonthReload(): void {
    if (this.monthReloadTimer) {
      clearTimeout(this.monthReloadTimer);
    }

    this.monthReloadTimer = setTimeout(() => {
      this.monthReloadTimer = null;
      void Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
    }, this.monthReloadDelayMs);
  }

  private applyPaymentFilters(): PaymentRow[] {
    const descFilter = this.descriptionFilter().toLowerCase().trim();
    const catFilter = this.categoryFilter();
    const minAmt = this.minAmount();
    const maxAmt = this.maxAmount();

    return this.payments().filter((p) => {
      if (catFilter && p.paymentCategoryId !== catFilter) return false;
      if (descFilter && !p.description.toLowerCase().includes(descFilter)) return false;
      const amount = Number(p.amount ?? 0);
      if (minAmt !== null && amount < minAmt) return false;
      if (maxAmt !== null && amount > maxAmt) return false;
      return true;
    });
  }

  private applyOccurrenceFilters(): ForecastOccurrenceRow[] {
    const descFilter = this.descriptionFilter().toLowerCase().trim();
    const catFilter = this.categoryFilter();
    const minAmt = this.minAmount();
    const maxAmt = this.maxAmount();

    return this.paymentOccurrences().filter((o) => {
      if (catFilter && o.paymentCategoryId !== catFilter) return false;
      if (descFilter && !o.description.toLowerCase().includes(descFilter)) return false;
      const amount = Number(o.amount ?? 0);
      if (minAmt !== null && amount < minAmt) return false;
      if (maxAmt !== null && amount > maxAmt) return false;
      return true;
    });
  }

  private getCurrentMonthInput(): string {
    const timezoneOffset = new Date().getTimezoneOffset() * 60000;
    return new Date(Date.now() - timezoneOffset).toISOString().slice(0, 7);
  }
}
