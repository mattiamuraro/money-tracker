import { Injectable, signal, computed, inject } from '@angular/core';
import { IncomeFormModel, IncomeRow } from './income.models';
import { ForecastOccurrenceRow } from '../forecasts/forecast.models';
import { DateUtilService } from '../../shared/services/date-util.service';

/**
 * Manages income form state using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class IncomeFormService {
  private readonly dateUtil = inject(DateUtilService);

  readonly editingId = signal<string | null>(null);
  readonly form = signal<IncomeFormModel>(this.createEmptyForm());
  readonly isEditing = computed(() => this.editingId() !== null);

  startEdit(income: IncomeRow): void {
    this.editingId.set(income.id);
    this.form.set({
      description: income.description,
      forecastOccurrenceId: income.forecastOccurrenceId ?? null,
      amount: income.amount,
      date: this.dateUtil.toInputDate(income.date),
    });
  }

  startFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.editingId.set(null);
    this.form.set({
      description: occurrence.description,
      forecastOccurrenceId: occurrence.id,
      amount: occurrence.amount,
      date: occurrence.expectedDate,
    });
  }

  updateForm(changes: Partial<IncomeFormModel>): void {
    this.form.update((current) => ({ ...current, ...changes }));
  }

  reset(): void {
    this.editingId.set(null);
    this.form.set(this.createEmptyForm());
  }

  clearForecastOccurrence(): void {
    this.form.update((f) => ({ ...f, forecastOccurrenceId: null }));
  }

  private createEmptyForm(): IncomeFormModel {
    return {
      description: '',
      forecastOccurrenceId: null,
      amount: null,
      date: this.dateUtil.toInputDate(new Date()),
    };
  }
}
