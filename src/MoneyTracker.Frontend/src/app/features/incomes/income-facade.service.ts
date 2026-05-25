import { Injectable, computed } from '@angular/core';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { DateUtilService } from '../../shared/services/date-util.service';
import { ForecastOccurrenceRow, OccurrenceDeleteAction } from '../forecasts/forecast.models';
import { IncomeDataService } from './income-data.service';
import { IncomeFormService } from './income-form.service';
import { IncomeUIService } from './income-ui.service';
import { IncomeFormModel, IncomeRow } from './income.models';

@Injectable({ providedIn: 'root' })
export class IncomeFacadeService {
  get incomes() { return this.data.incomes; }
  get incomeOccurrences() { return this.data.incomeOccurrences; }
  get filteredIncomes() { return this.data.filteredIncomes; }
  get filteredOccurrences() { return this.data.filteredOccurrences; }
  get incomeTotal() { return this.data.incomeTotal; }
  get selectedMonth() { return this.data.selectedMonth; }

  readonly incomeFilters = computed(() => ({
    descriptionFilter: this.data.descriptionFilter(),
    minAmount: this.data.minAmount(),
    maxAmount: this.data.maxAmount(),
  }));

  get incomeForm() { return this.form.form; }
  get isEditingIncome() { return this.form.isEditing; }

  get isSavingIncome() { return this.ui.isSavingIncome; }
  get successMessage() { return this.ui.successMessage; }
  get errorMessage() { return this.ui.errorMessage; }

  constructor(
    private readonly confirmationDialogService: ConfirmationDialogService,
    private readonly dateUtil: DateUtilService,
    private readonly data: IncomeDataService,
    private readonly form: IncomeFormService,
    private readonly ui: IncomeUIService
  ) {}

  async submitIncome(): Promise<void> {
    const formValue = this.form.form();

    if (!formValue.description.trim() || !formValue.amount || !formValue.date) {
      this.ui.setErrorMessage('Complete all required income fields before saving.');
      return;
    }

    this.ui.setSavingIncome(true);
    this.ui.clearMessages();

    try {
      const model: IncomeFormModel = {
        ...formValue,
        description: formValue.description.trim(),
        forecastOccurrenceId: formValue.forecastOccurrenceId || null,
      };

      if (this.form.isEditing()) {
        await this.data.updateIncome(this.form.editingId()!, model);
        this.ui.setSuccessMessage('Income updated successfully.');
      } else {
        await this.data.createIncome(model);
        this.ui.setSuccessMessage('Income created successfully.');
      }

      this.form.reset();
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to save the income.'));
    } finally {
      this.ui.setSavingIncome(false);
    }
  }

  async deleteIncome(income: IncomeRow): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Income',
      message: `Delete income "${income.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      const occurrenceAction = await this.resolveOccurrenceDeleteAction(
        income.description,
        income.forecastOccurrenceId,
        income.forecastExpectedDate
      );
      await this.data.deleteIncome(income.id, occurrenceAction);
      this.ui.setSuccessMessage('Income deleted successfully.');

      if (this.form.editingId() === income.id) {
        this.form.reset();
      }
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to delete the income.'));
    }
  }

  startIncomeEdit(income: IncomeRow): void {
    this.form.startEdit(income);
    this.ui.clearMessages();
  }

  startIncomeFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.form.startFromOccurrence(occurrence);
    this.ui.clearMessages();
  }

  cancelIncomeEdit(): void {
    this.form.reset();
    this.ui.clearMessages();
  }

  resetIncomeForm(): void {
    this.form.reset();
  }

  updateIncomeForm(changes: Partial<IncomeFormModel>): void {
    this.form.updateForm(changes);
  }

  clearIncomeForecastOccurrence(): void {
    this.form.clearForecastOccurrence();
  }

  onMonthChanged(month: string): void {
    this.data.setSelectedMonth(month);
  }

  setDescriptionFilter(description: string): void {
    this.data.setDescriptionFilter(description);
  }

  setAmountRange(min: number | null, max: number | null): void {
    this.data.setAmountRange(min, max);
  }

  applyIncomeFilters(): void {
    this.data.setSelectedMonth(this.data.selectedMonth());
  }

  resetFilters(): void {
    this.data.resetFilters();
  }

  resetIncomeFilters(): void {
    this.resetFilters();
  }

  async loadAll(): Promise<void> {
    await Promise.all([
      this.data.loadIncomes(),
      this.data.loadIncomeOccurrences(),
    ]);
  }

  updateIncomeFilters(filters: { descriptionFilter?: string; minAmount?: number | null; maxAmount?: number | null }): void {
    if (filters.descriptionFilter !== undefined) {
      this.setDescriptionFilter(filters.descriptionFilter);
    }

    const minAmount = filters.minAmount !== undefined ? filters.minAmount : this.data.minAmount();
    const maxAmount = filters.maxAmount !== undefined ? filters.maxAmount : this.data.maxAmount();
    this.setAmountRange(minAmount, maxAmount);
  }

  private async resolveOccurrenceDeleteAction(
    description: string,
    forecastOccurrenceId?: string | null,
    forecastExpectedDate?: string | null
  ): Promise<OccurrenceDeleteAction | undefined> {
    if (!forecastOccurrenceId || !forecastExpectedDate || !this.dateUtil.isPastDate(forecastExpectedDate)) {
      return undefined;
    }

    const reopen = await this.confirmationDialogService.show({
      title: 'Forecast Occurrence Handling',
      message: `"${description}" is linked to a past forecast occurrence scheduled on ${this.dateUtil.toDisplayDate(forecastExpectedDate)}. Move it back to pending?`,
      confirmButtonText: 'Move to pending',
      cancelButtonText: 'Skip occurrence',
      isDangerous: false,
    });

    return reopen ? 'Reopen' : 'Skip';
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof Error && error.message) return error.message;
    return fallbackMessage;
  }
}
