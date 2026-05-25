import { Injectable, signal, computed, inject } from '@angular/core';
import { ForecastIncomeFormModel, ForecastExpenseFormModel, ForecastIncomeDefinition, ForecastExpenseDefinition } from './forecast.models';
import { DateUtilService } from '../../shared/services/date-util.service';

/**
 * Manages forecast form state using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class ForecastFormService {
  private readonly dateUtil = inject(DateUtilService);

  readonly editingIncomeId = signal<string | null>(null);
  readonly incomeForm = signal<ForecastIncomeFormModel>(this.createEmptyIncomeForm());
  readonly isEditingIncome = computed(() => this.editingIncomeId() !== null);

  readonly editingExpenseId = signal<string | null>(null);
  readonly expenseForm = signal<ForecastExpenseFormModel>(this.createEmptyExpenseForm());
  readonly isEditingExpense = computed(() => this.editingExpenseId() !== null);

  startIncomeEdit(definition: ForecastIncomeDefinition): void {
    this.editingIncomeId.set(definition.id);
    this.incomeForm.set({
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.dateUtil.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.dateUtil.toInputDate(definition.recurrenceEnd) : '',
      interval: definition.interval,
    });
  }

  updateIncomeForm(changes: Partial<ForecastIncomeFormModel>): void {
    this.incomeForm.update((current) => ({ ...current, ...changes }));
  }

  resetIncomeForm(): void {
    this.editingIncomeId.set(null);
    this.incomeForm.set(this.createEmptyIncomeForm());
  }

  startExpenseEdit(definition: ForecastExpenseDefinition): void {
    this.editingExpenseId.set(definition.id);
    this.expenseForm.set({
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.dateUtil.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.dateUtil.toInputDate(definition.recurrenceEnd) : '',
      interval: definition.interval,
      paymentCategoryId: definition.paymentCategoryId ?? '',
    });
  }

  updateExpenseForm(changes: Partial<ForecastExpenseFormModel>): void {
    this.expenseForm.update((current) => ({ ...current, ...changes }));
  }

  resetExpenseForm(): void {
    this.editingExpenseId.set(null);
    this.expenseForm.set(this.createEmptyExpenseForm());
  }

  private createEmptyIncomeForm(): ForecastIncomeFormModel {
    return {
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: this.dateUtil.toInputDate(new Date()),
      recurrenceEnd: '',
      interval: 1,
    };
  }

  private createEmptyExpenseForm(): ForecastExpenseFormModel {
    return {
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: this.dateUtil.toInputDate(new Date()),
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: '',
    };
  }
}
