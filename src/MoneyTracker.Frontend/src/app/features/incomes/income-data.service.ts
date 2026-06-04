import { Injectable, signal, computed } from '@angular/core';
import { MoneyTrackerApiService } from '../../money-tracker-api.service';
import { IncomeFormModel, IncomeQuery, IncomeRow } from './income.models';
import { ForecastOccurrenceRow, OccurrenceDeleteAction } from '../forecasts/forecast.models';

/**
 * Manages income data state using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class IncomeDataService {
  readonly incomes = signal<IncomeRow[]>([]);
  readonly incomeOccurrences = signal<ForecastOccurrenceRow[]>([]);
  readonly selectedMonth = signal<string>(this.getCurrentMonthInput());
  readonly descriptionFilter = signal<string>('');
  readonly minAmount = signal<number | null>(null);
  readonly maxAmount = signal<number | null>(null);

  readonly filteredIncomes = computed(() => this.applyIncomeFilters());
  readonly filteredOccurrences = computed(() => this.applyOccurrenceFilters());
  readonly incomeTotal = computed(() =>
    this.incomes().reduce((total, i) => total + Number(i.amount ?? 0), 0)
  );

  private readonly pageSize = 100;
  private monthReloadTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly monthReloadDelayMs = 250;

  constructor(private readonly apiService: MoneyTrackerApiService) {}

  async loadIncomes(): Promise<void> {
    try {
      const baseQuery: IncomeQuery = {
        month: this.selectedMonth(),
        pageNumber: 1,
        pageSize: this.pageSize,
        sortBy: 'Date',
        sortOrder: 'asc',
      };

      const firstPage = await this.apiService.getIncomes(baseQuery);
      const allItems = [...firstPage.items];
      const totalPages = Math.max(1, Math.ceil(firstPage.totalItems / this.pageSize));

      for (let page = 2; page <= totalPages; page++) {
        const nextPage = await this.apiService.getIncomes({ ...baseQuery, pageNumber: page });
        allItems.push(...nextPage.items);
      }

      this.incomes.set([...allItems].sort((a, b) => a.date.localeCompare(b.date)));
    } catch (error) {
      console.error('Failed to load incomes:', error);
      throw error;
    }
  }

  async loadIncomeOccurrences(): Promise<void> {
    try {
      const occurrences = await this.apiService.getForecastIncomeOccurrences(this.selectedMonth());
      this.incomeOccurrences.set([...occurrences].sort((a, b) => a.expectedDate.localeCompare(b.expectedDate)));
    } catch (error) {
      console.error('Failed to load income occurrences:', error);
      throw error;
    }
  }

  async createIncome(model: IncomeFormModel): Promise<string> {
    const id = await this.apiService.createIncome(model);
    await Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
    return id;
  }

  async updateIncome(id: string, model: IncomeFormModel): Promise<void> {
    await this.apiService.updateIncome(id, model);
    await Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  async deleteIncome(id: string, occurrenceAction?: OccurrenceDeleteAction): Promise<void> {
    await this.apiService.deleteIncome(id, occurrenceAction);
    await Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  setSelectedMonth(month: string): void {
    this.selectedMonth.set(month);
    this.scheduleMonthReload();
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
    this.descriptionFilter.set('');
    this.minAmount.set(null);
    this.maxAmount.set(null);
    void Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  private scheduleMonthReload(): void {
    if (this.monthReloadTimer) {
      clearTimeout(this.monthReloadTimer);
    }

    this.monthReloadTimer = setTimeout(() => {
      this.monthReloadTimer = null;
      void Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
    }, this.monthReloadDelayMs);
  }

  private applyIncomeFilters(): IncomeRow[] {
    const descFilter = this.descriptionFilter().toLowerCase().trim();
    const minAmt = this.minAmount();
    const maxAmt = this.maxAmount();

    return this.incomes().filter((i) => {
      if (descFilter && !i.description.toLowerCase().includes(descFilter)) return false;
      const amount = Number(i.amount ?? 0);
      if (minAmt !== null && amount < minAmt) return false;
      if (maxAmt !== null && amount > maxAmt) return false;
      return true;
    });
  }

  private applyOccurrenceFilters(): ForecastOccurrenceRow[] {
    const descFilter = this.descriptionFilter().toLowerCase().trim();
    const minAmt = this.minAmount();
    const maxAmt = this.maxAmount();

    return this.incomeOccurrences().filter((o) => {
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
