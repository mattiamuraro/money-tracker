import { Injectable } from '@angular/core';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { IncomeDataService } from '../incomes/income-data.service';
import { PaymentDataService } from '../payments/payment-data.service';
import { ForecastDataService } from './forecast-data.service';
import { ForecastFormService } from './forecast-form.service';
import { ForecastUIService } from './forecast-ui.service';
import {
  ForecastExpenseDefinition,
  ForecastExpenseFormModel,
  ForecastIncomeDefinition,
  ForecastIncomeFormModel,
  ForecastOccurrenceRow,
} from './forecast.models';

@Injectable({ providedIn: 'root' })
export class ForecastFacadeService {
  get recurrenceRuleTypes() { return this.data.recurrenceRuleTypes; }
  get incomeDefinitions() { return this.data.incomeDefinitions; }
  get expenseDefinitions() { return this.data.expenseDefinitions; }
  get incomeRows() { return this.data.incomeRows; }
  get expenseRows() { return this.data.expenseRows; }
  get incomeTotal() { return this.data.incomeTotal; }
  get expenseTotal() { return this.data.expenseTotal; }
  get balance() { return this.data.balance; }
  get upcomingIncomes() { return this.data.upcomingIncomes; }
  get upcomingExpenses() { return this.data.upcomingExpenses; }
  get rangeStart() { return this.data.rangeStart; }
  get rangeEnd() { return this.data.rangeEnd; }

  get incomeForm() { return this.form.incomeForm; }
  get isEditingIncome() { return this.form.isEditingIncome; }
  get expenseForm() { return this.form.expenseForm; }
  get isEditingExpense() { return this.form.isEditingExpense; }

  get isSavingIncome() { return this.ui.isSavingIncome; }
  get isSavingExpense() { return this.ui.isSavingExpense; }
  get successMessage() { return this.ui.successMessage; }
  get errorMessage() { return this.ui.errorMessage; }

  constructor(
    private readonly confirmationDialogService: ConfirmationDialogService,
    private readonly data: ForecastDataService,
    private readonly form: ForecastFormService,
    private readonly ui: ForecastUIService,
    private readonly paymentData: PaymentDataService,
    private readonly incomeData: IncomeDataService
  ) {}

  async submitIncomeDefinition(): Promise<void> {
    const formValue = this.form.incomeForm();
    const selectedRecurrenceTypeId = formValue.forecastRecurrenceRuleTypeId.trim();
    const isOneTime = this.data.isOneTimeRecurrenceType(selectedRecurrenceTypeId);

    if (!selectedRecurrenceTypeId || !formValue.description.trim() || !formValue.amount || !formValue.recurrenceStart || (!isOneTime && formValue.interval < 1)) {
      this.ui.setErrorMessage('Complete all required forecast income fields before saving.');
      return;
    }

    this.ui.setSavingIncome(true);
    this.ui.clearMessages();

    try {
      const model: ForecastIncomeFormModel = {
        ...formValue,
        forecastRecurrenceRuleTypeId: selectedRecurrenceTypeId,
        interval: isOneTime ? 1 : formValue.interval,
        description: formValue.description.trim(),
      };

      if (this.form.isEditingIncome()) {
        await this.data.updateIncomeDefinition(this.form.editingIncomeId()!, model);
        this.ui.setSuccessMessage('Forecast income updated successfully.');
      } else {
        await this.data.createIncomeDefinition(model);
        this.ui.setSuccessMessage('Forecast income created successfully.');
      }

      await this.incomeData.loadIncomeOccurrences();
      this.form.resetIncomeForm();
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to save the forecast income.'));
    } finally {
      this.ui.setSavingIncome(false);
    }
  }

  async deleteIncomeDefinition(definition: ForecastIncomeDefinition): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Forecast Income',
      message: `Delete forecast income "${definition.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      await this.data.deleteIncomeDefinition(definition.id);
      await this.incomeData.loadIncomeOccurrences();
      this.ui.setSuccessMessage('Forecast income deleted successfully.');

      if (this.form.editingIncomeId() === definition.id) {
        this.form.resetIncomeForm();
      }
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to delete the forecast income.'));
    }
  }

  startIncomeEdit(definition: ForecastIncomeDefinition): void {
    this.form.startIncomeEdit(definition);
    this.ui.clearMessages();
  }

  cancelIncomeEdit(): void {
    this.form.resetIncomeForm();
    this.ui.clearMessages();
  }

  updateIncomeForm(changes: Partial<ForecastIncomeFormModel>): void {
    this.form.updateIncomeForm(changes);
  }

  onIncomeRecurrenceTypeChanged(): void {
    const formValue = this.form.incomeForm();
    if (this.data.isOneTimeRecurrenceType(formValue.forecastRecurrenceRuleTypeId)) {
      this.updateIncomeForm({ interval: 1 });
    }
  }

  async submitExpenseDefinition(): Promise<void> {
    const formValue = this.form.expenseForm();
    const selectedRecurrenceTypeId = formValue.forecastRecurrenceRuleTypeId.trim();
    const isOneTime = this.data.isOneTimeRecurrenceType(selectedRecurrenceTypeId);

    if (!selectedRecurrenceTypeId || !formValue.description.trim() || !formValue.amount || !formValue.recurrenceStart || (!isOneTime && formValue.interval < 1)) {
      this.ui.setErrorMessage('Complete all required forecast expense fields before saving.');
      return;
    }

    if (!formValue.paymentCategoryId) {
      this.ui.setErrorMessage('Forecast expenses require a payment category.');
      return;
    }

    this.ui.setSavingExpense(true);
    this.ui.clearMessages();

    try {
      const model: ForecastExpenseFormModel = {
        ...formValue,
        forecastRecurrenceRuleTypeId: selectedRecurrenceTypeId,
        interval: isOneTime ? 1 : formValue.interval,
        description: formValue.description.trim(),
      };

      if (this.form.isEditingExpense()) {
        await this.data.updateExpenseDefinition(this.form.editingExpenseId()!, model);
        this.ui.setSuccessMessage('Forecast expense updated successfully.');
      } else {
        await this.data.createExpenseDefinition(model);
        this.ui.setSuccessMessage('Forecast expense created successfully.');
      }

      await this.paymentData.loadPaymentOccurrences();
      this.form.resetExpenseForm();
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to save the forecast expense.'));
    } finally {
      this.ui.setSavingExpense(false);
    }
  }

  async deleteExpenseDefinition(definition: ForecastExpenseDefinition): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Forecast Expense',
      message: `Delete forecast expense "${definition.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      await this.data.deleteExpenseDefinition(definition.id);
      await this.paymentData.loadPaymentOccurrences();
      this.ui.setSuccessMessage('Forecast expense deleted successfully.');

      if (this.form.editingExpenseId() === definition.id) {
        this.form.resetExpenseForm();
      }
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to delete the forecast expense.'));
    }
  }

  startExpenseEdit(definition: ForecastExpenseDefinition): void {
    this.form.startExpenseEdit(definition);
    this.ui.clearMessages();
  }

  cancelExpenseEdit(): void {
    this.form.resetExpenseForm();
    this.ui.clearMessages();
  }

  updateExpenseForm(changes: Partial<ForecastExpenseFormModel>): void {
    this.form.updateExpenseForm(changes);
  }

  resetIncomeForm(): void {
    this.form.resetIncomeForm();
  }

  resetExpenseForm(): void {
    this.form.resetExpenseForm();
  }

  onExpenseRecurrenceTypeChanged(): void {
    const formValue = this.form.expenseForm();
    if (this.data.isOneTimeRecurrenceType(formValue.forecastRecurrenceRuleTypeId)) {
      this.updateExpenseForm({ interval: 1 });
    }
  }

  getRecurrenceTypeLabel(typeId: string): string {
    return this.data.getRecurrenceTypeLabel(typeId);
  }

  getRecurrenceSummary(typeId: string, interval: number): string {
    return this.data.getRecurrenceSummary(typeId, interval);
  }

  isOneTimeRecurrenceTypeSelected(): boolean {
    return this.data.isOneTimeRecurrenceType(this.form.expenseForm().forecastRecurrenceRuleTypeId);
  }

  isOneTimeForecastIncomeRecurrenceTypeSelected(): boolean {
    return this.data.isOneTimeRecurrenceType(this.form.incomeForm().forecastRecurrenceRuleTypeId);
  }

  async discardForecastOccurrence(occurrence: ForecastOccurrenceRow, isIncome: boolean): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Discard Forecast Occurrence',
      message: `Discard pending forecast occurrence "${occurrence.description}" scheduled on ${occurrence.expectedDate.slice(0, 10)}?`,
      confirmButtonText: 'Discard',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      if (isIncome) {
        await this.data.discardIncomeOccurrence(occurrence.id);
        await Promise.all([this.incomeData.loadIncomeOccurrences(), this.incomeData.loadIncomes(), this.data.loadRows()]);
      } else {
        await this.data.discardExpenseOccurrence(occurrence.id);
        await Promise.all([this.paymentData.loadPaymentOccurrences(), this.paymentData.loadPayments(), this.data.loadRows()]);
      }

      this.ui.setSuccessMessage('Forecast occurrence discarded successfully.');
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to discard the forecast occurrence.'));
    }
  }

  setDateRange(start: string, end: string): void {
    this.data.setDateRange(start, end);
  }

  async loadAll(force = false): Promise<void> {
    await Promise.all([
      this.data.loadRecurrenceRuleTypes(force),
      this.data.loadDefinitions(),
      this.data.loadRows(),
    ]);
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof Error && error.message) return error.message;
    return fallbackMessage;
  }
}
