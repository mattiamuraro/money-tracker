import { Injectable, signal, computed } from '@angular/core';
import { MoneyTrackerApiService } from '../../money-tracker-api.service';
import { DateUtilService } from '../../shared/services/date-util.service';
import {
  ForecastExpenseDefinition,
  ForecastExpenseFormModel,
  ForecastExpenseRow,
  ForecastIncomeDefinition,
  ForecastIncomeFormModel,
  ForecastIncomeRow,
  ForecastOccurrenceRow,
  ForecastRecurrenceRuleTypeOption,
} from './forecast.models';

/**
 * Manages forecast data state using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class ForecastDataService {
  readonly recurrenceRuleTypes = signal<ForecastRecurrenceRuleTypeOption[]>([]);
  readonly incomeDefinitions = signal<ForecastIncomeDefinition[]>([]);
  readonly expenseDefinitions = signal<ForecastExpenseDefinition[]>([]);
  readonly incomeRows = signal<ForecastIncomeRow[]>([]);
  readonly expenseRows = signal<ForecastExpenseRow[]>([]);

  readonly rangeStart = signal<string>(this.getInputDate(new Date()));
  readonly rangeEnd = signal<string>(this.getInputDate(this.addDays(new Date(), 30)));

  readonly incomeTotal = computed(() =>
    this.incomeRows().reduce((total, row) => total + Number(row.amount ?? 0), 0)
  );
  readonly expenseTotal = computed(() =>
    this.expenseRows().reduce((total, row) => total + Number(row.amount ?? 0), 0)
  );
  readonly balance = computed(() => this.incomeTotal() - this.expenseTotal());
  readonly upcomingIncomes = computed(() => this.incomeRows().slice(0, 5));
  readonly upcomingExpenses = computed(() => this.expenseRows().slice(0, 5));

  private rangeReloadTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly rangeReloadDelayMs = 250;
  private hasLoadedRecurrenceRuleTypes = false;

  constructor(
    private readonly apiService: MoneyTrackerApiService,
    private readonly dateUtil: DateUtilService
  ) {}

  async loadRecurrenceRuleTypes(force = false): Promise<void> {
    if (!force && this.hasLoadedRecurrenceRuleTypes && this.recurrenceRuleTypes().length > 0) {
      return;
    }

    try {
      const types = await this.apiService.getForecastRecurrenceRuleTypes();
      this.recurrenceRuleTypes.set([...types].sort((a, b) => a.name.localeCompare(b.name)));
      this.hasLoadedRecurrenceRuleTypes = true;
    } catch (error) {
      console.error('Failed to load recurrence rule types:', error);
      throw error;
    }
  }

  async loadDefinitions(): Promise<void> {
    try {
      const [incomes, expenses] = await Promise.all([
        this.apiService.getForecastIncomeDefinitions(),
        this.apiService.getForecastExpenseDefinitions(),
      ]);

      this.incomeDefinitions.set([...incomes].sort((a, b) => {
        const dateComparison = a.recurrenceStart.localeCompare(b.recurrenceStart);
        return dateComparison !== 0 ? dateComparison : a.description.localeCompare(b.description);
      }));

      this.expenseDefinitions.set([...expenses].sort((a, b) => {
        const dateComparison = a.recurrenceStart.localeCompare(b.recurrenceStart);
        return dateComparison !== 0 ? dateComparison : a.description.localeCompare(b.description);
      }));
    } catch (error) {
      console.error('Failed to load forecast definitions:', error);
      throw error;
    }
  }

  async loadRows(): Promise<void> {
    try {
      const [incomes, expenses] = await Promise.all([
        this.apiService.getForecastIncomeRows(this.rangeStart(), this.rangeEnd()),
        this.apiService.getForecastExpenseRows(this.rangeStart(), this.rangeEnd()),
      ]);

      this.incomeRows.set([...incomes].sort((a, b) => a.date.localeCompare(b.date)));
      this.expenseRows.set([...expenses].sort((a, b) => a.date.localeCompare(b.date)));
    } catch (error) {
      console.error('Failed to load forecast rows:', error);
      throw error;
    }
  }

  async createIncomeDefinition(model: ForecastIncomeFormModel): Promise<string> {
    const id = await this.apiService.createForecastIncomeDefinition(model);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
    return id;
  }

  async updateIncomeDefinition(id: string, model: ForecastIncomeFormModel): Promise<void> {
    await this.apiService.updateForecastIncomeDefinition(id, model);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
  }

  async deleteIncomeDefinition(id: string): Promise<void> {
    await this.apiService.deleteForecastIncomeDefinition(id);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
  }

  async createExpenseDefinition(model: ForecastExpenseFormModel): Promise<string> {
    const id = await this.apiService.createForecastExpenseDefinition(model);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
    return id;
  }

  async updateExpenseDefinition(id: string, model: ForecastExpenseFormModel): Promise<void> {
    await this.apiService.updateForecastExpenseDefinition(id, model);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
  }

  async deleteExpenseDefinition(id: string): Promise<void> {
    await this.apiService.deleteForecastExpenseDefinition(id);
    await Promise.all([this.loadDefinitions(), this.loadRows()]);
  }

  async discardIncomeOccurrence(id: string): Promise<void> {
    await this.apiService.discardForecastIncomeOccurrence(id);
  }

  async discardExpenseOccurrence(id: string): Promise<void> {
    await this.apiService.discardForecastExpenseOccurrence(id);
  }

  setDateRange(start: string, end: string): void {
    this.rangeStart.set(start);
    this.rangeEnd.set(end);

    if (this.rangeReloadTimer) {
      clearTimeout(this.rangeReloadTimer);
    }

    this.rangeReloadTimer = setTimeout(() => {
      this.rangeReloadTimer = null;
      void this.loadRows();
    }, this.rangeReloadDelayMs);
  }

  getRecurrenceTypeLabel(typeId: string): string {
    const type = this.recurrenceRuleTypes().find((t) => t.id === typeId);
    return type ? `${type.name} (${type.code})` : typeId;
  }

  isOneTimeRecurrenceType(typeId: string): boolean {
    const type = this.recurrenceRuleTypes().find((t) => t.id === typeId);
    return type?.code === 'O';
  }

  getRecurrenceSummary(typeId: string, interval: number): string {
    const type = this.recurrenceRuleTypes().find((t) => t.id === typeId);
    if (!type) {
      return interval === 1 ? 'Every 1 occurrence' : `Every ${interval} occurrences`;
    }

    switch (type.code) {
      case 'O':
        return 'One time';
      case 'D':
        return `Every ${interval} day${interval > 1 ? 's' : ''}`;
      case 'W':
        return `Every ${interval} week${interval > 1 ? 's' : ''}`;
      case 'M':
        return `Every ${interval} month${interval > 1 ? 's' : ''}`;
      case 'Y':
        return `Every ${interval} year${interval > 1 ? 's' : ''}`;
      default:
        return `Every ${interval}`;
    }
  }

  private getInputDate(value: Date): string {
    const timezoneOffset = value.getTimezoneOffset() * 60000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 10);
  }

  private addDays(value: Date, days: number): Date {
    const result = new Date(value);
    result.setDate(result.getDate() + days);
    return result;
  }
}
