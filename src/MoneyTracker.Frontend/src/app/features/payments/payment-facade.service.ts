import { Injectable, computed } from '@angular/core';
import { MoneyTrackerApiService } from '../../money-tracker-api.service';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { DateUtilService } from '../../shared/services/date-util.service';
import { ForecastOccurrenceRow, OccurrenceDeleteAction } from '../forecasts/forecast.models';
import { PaymentDataService } from './payment-data.service';
import { PaymentFormService } from './payment-form.service';
import { CategoryFormService } from './category-form.service';
import { PaymentUIService } from './payment-ui.service';
import { PaymentCategory, PaymentCategoryFormModel, PaymentFormModel, PaymentRow } from './payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentFacadeService {
  get categories() { return this.data.categories; }
  get payments() { return this.data.payments; }
  get paymentOccurrences() { return this.data.paymentOccurrences; }
  get filteredPayments() { return this.data.filteredPayments; }
  get filteredOccurrences() { return this.data.filteredOccurrences; }
  get paymentTotal() { return this.data.paymentTotal; }
  get combinedTotal() { return this.data.combinedTotal; }
  get selectedMonth() { return this.data.selectedMonth; }

  readonly hasCategories = computed(() => this.data.categories().length > 0);

  readonly paymentFilters = computed(() => ({
    categoryId: this.data.categoryFilter(),
    descriptionFilter: this.data.descriptionFilter(),
    minAmount: this.data.minAmount(),
    maxAmount: this.data.maxAmount(),
  }));

  get paymentForm() { return this.form.form; }
  get isEditingPayment() { return this.form.isEditing; }
  get categoryForm() { return this.categoryFormService.form; }
  get isEditingCategory() { return this.categoryFormService.isEditing; }

  get isSavingPayment() { return this.ui.isSavingPayment; }
  get isSavingCategory() { return this.ui.isSavingCategory; }
  get successMessage() { return this.ui.successMessage; }
  get errorMessage() { return this.ui.errorMessage; }

  constructor(
    private readonly apiService: MoneyTrackerApiService,
    private readonly confirmationDialogService: ConfirmationDialogService,
    private readonly dateUtil: DateUtilService,
    private readonly data: PaymentDataService,
    private readonly form: PaymentFormService,
    private readonly categoryFormService: CategoryFormService,
    private readonly ui: PaymentUIService
  ) {}

  async submitPayment(): Promise<void> {
    const formValue = this.form.form();

    if (!formValue.description.trim() || !formValue.paymentCategoryId || !formValue.amount || !formValue.date) {
      this.ui.setErrorMessage('Complete all required payment fields before saving.');
      return;
    }

    this.ui.setSavingPayment(true);
    this.ui.clearMessages();

    try {
      const model: PaymentFormModel = {
        ...formValue,
        description: formValue.description.trim(),
        forecastOccurrenceId: formValue.forecastOccurrenceId || null,
      };

      if (this.form.isEditing()) {
        await this.data.updatePayment(this.form.editingId()!, model);
        this.ui.setSuccessMessage('Payment updated successfully.');
      } else {
        await this.data.createPayment(model);
        this.ui.setSuccessMessage('Payment created successfully.');
      }

      this.form.reset();
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to save the payment.'));
    } finally {
      this.ui.setSavingPayment(false);
    }
  }

  async deletePayment(payment: PaymentRow): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Payment',
      message: `Delete payment "${payment.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      const occurrenceAction = await this.resolveOccurrenceDeleteAction(
        payment.description,
        payment.forecastOccurrenceId,
        payment.forecastExpectedDate
      );
      await this.data.deletePayment(payment.id, occurrenceAction);
      this.ui.setSuccessMessage('Payment deleted successfully.');

      if (this.form.editingId() === payment.id) {
        this.form.reset();
      }
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to delete the payment.'));
    }
  }

  startPaymentEdit(payment: PaymentRow): void {
    this.form.startEdit(payment);
    this.ui.clearMessages();
  }

  startPaymentFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.form.startFromOccurrence(occurrence);
    this.ui.clearMessages();
  }

  cancelPaymentEdit(): void {
    this.form.reset();
    this.ui.clearMessages();
  }

  resetPaymentForm(): void {
    this.form.reset();
  }

  updatePaymentForm(changes: Partial<PaymentFormModel>): void {
    this.form.updateForm(changes);
  }

  clearPaymentForecastOccurrence(): void {
    this.form.clearForecastOccurrence();
  }

  async submitCategory(): Promise<void> {
    const formValue = this.categoryFormService.form();
    const name = formValue.name.trim();
    const code = formValue.code.trim().toUpperCase();

    if (!name || !code) {
      this.ui.setErrorMessage('Complete all required category fields before saving.');
      return;
    }

    this.ui.setSavingCategory(true);
    this.ui.clearMessages();

    try {
      const model: PaymentCategoryFormModel = { name, code };

      if (this.categoryFormService.isEditing()) {
        await this.apiService.updateCategory(this.categoryFormService.editingId()!, model);
        this.ui.setSuccessMessage('Category updated successfully.');
      } else {
        await this.apiService.createCategory(model);
        this.ui.setSuccessMessage('Category created successfully.');
      }

      await this.data.loadCategories();
      this.categoryFormService.reset();
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to save the category.'));
    } finally {
      this.ui.setSavingCategory(false);
    }
  }

  async deleteCategory(category: PaymentCategory): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Category',
      message: `Delete category "${category.name}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) return;

    this.ui.clearMessages();

    try {
      await this.apiService.deleteCategory(category.id);
      await this.data.loadCategories();
      this.ui.setSuccessMessage('Category deleted successfully.');

      if (this.categoryFormService.editingId() === category.id) {
        this.categoryFormService.reset();
      }

      if (this.form.form().paymentCategoryId === category.id) {
        this.form.updateForm({ paymentCategoryId: '' });
      }
    } catch (error) {
      this.ui.setErrorMessage(this.getErrorMessage(error, 'Unable to delete the category.'));
    }
  }

  startCategoryEdit(category: PaymentCategory): void {
    this.categoryFormService.startEdit(category);
    this.ui.clearMessages();
  }

  cancelCategoryEdit(): void {
    this.categoryFormService.reset();
    this.ui.clearMessages();
  }

  resetCategoryForm(): void {
    this.categoryFormService.reset();
  }

  updateCategoryForm(changes: Partial<PaymentCategoryFormModel>): void {
    this.categoryFormService.updateForm(changes);
  }

  onMonthChanged(month: string): void {
    this.data.setSelectedMonth(month);
  }

  onPaymentMonthChanged(month: string): void {
    this.onMonthChanged(month);
  }

  setCategoryFilter(categoryId: string): void {
    this.data.setCategoryFilter(categoryId);
  }

  setDescriptionFilter(description: string): void {
    this.data.setDescriptionFilter(description);
  }

  setAmountRange(min: number | null, max: number | null): void {
    this.data.setAmountRange(min, max);
  }

  applyPaymentFilters(): void {
    this.data.setSelectedMonth(this.data.selectedMonth());
  }

  resetFilters(): void {
    this.data.resetFilters();
  }

  resetPaymentFilters(): void {
    this.resetFilters();
  }

  async loadAll(force = false): Promise<void> {
    await Promise.all([
      this.data.loadCategories(force),
      this.data.loadPayments(),
      this.data.loadPaymentOccurrences(),
    ]);
  }

  async loadCategoriesOnly(force = false): Promise<void> {
    await this.data.loadCategories(force);
  }

  updatePaymentFilters(filters: { categoryId?: string; descriptionFilter?: string; minAmount?: number | null; maxAmount?: number | null }): void {
    if (filters.categoryId !== undefined) {
      this.setCategoryFilter(filters.categoryId);
    }
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
