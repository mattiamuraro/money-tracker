import { Injectable, signal, computed, inject } from '@angular/core';
import { PaymentFormModel, PaymentRow } from './payment.models';
import { ForecastOccurrenceRow } from '../forecasts/forecast.models';
import { DateUtilService } from '../../shared/services/date-util.service';

/**
 * Manages payment form state using Angular signals.
 * Responsible for:
 * - Form model state (create, edit, reset)
 * - Editing context tracking
 * - Form validation helpers
 */
@Injectable({ providedIn: 'root' })
export class PaymentFormService {
  private readonly dateUtil = inject(DateUtilService);

  readonly editingId = signal<string | null>(null);
  readonly form = signal<PaymentFormModel>(this.createEmptyForm());
  readonly isEditing = computed(() => this.editingId() !== null);

  startEdit(payment: PaymentRow): void {
    this.editingId.set(payment.id);
    this.form.set({
      description: payment.description,
      paymentCategoryId: payment.paymentCategoryId,
      forecastOccurrenceId: payment.forecastOccurrenceId ?? null,
      amount: payment.amount,
      date: this.dateUtil.toInputDate(payment.date),
      isOneShot: payment.isOneShot,
    });
  }

  startFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.editingId.set(null);
    this.form.set({
      description: occurrence.description,
      paymentCategoryId: occurrence.paymentCategoryId ?? '',
      forecastOccurrenceId: occurrence.id,
      amount: occurrence.amount,
      date: occurrence.expectedDate,
      isOneShot: true,
    });
  }

  updateForm(changes: Partial<PaymentFormModel>): void {
    this.form.update((current) => ({ ...current, ...changes }));
  }

  reset(): void {
    this.editingId.set(null);
    this.form.set(this.createEmptyForm());
  }

  clearForecastOccurrence(): void {
    this.form.update((f) => ({ ...f, forecastOccurrenceId: null }));
  }

  private createEmptyForm(): PaymentFormModel {
    return {
      description: '',
      paymentCategoryId: '',
      forecastOccurrenceId: null,
      amount: null,
      date: this.dateUtil.toInputDate(new Date()),
      isOneShot: true,
    };
  }
}
