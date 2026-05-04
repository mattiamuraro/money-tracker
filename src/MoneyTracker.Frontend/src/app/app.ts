import { Component, NgZone, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './auth/auth.service';
import { MoneyTrackerApiService } from './money-tracker-api.service';
import { ConfirmationDialogService } from './services/confirmation-dialog.service';
import {
  ForecastExpenseDefinition,
  ForecastExpenseFormModel,
  ForecastExpenseRow,
  ForecastIncomeDefinition,
  ForecastIncomeFormModel,
  ForecastIncomeRow,
  ForecastOccurrenceRow,
  ForecastRecurrenceRuleTypeOption,
  IncomeFormModel,
  IncomeQuery,
  IncomeRow,
  OccurrenceDeleteAction,
  PaymentCategory,
  PaymentCategoryFormModel,
  PaymentFormModel,
  PaymentQuery,
  PaymentRow,
} from './models';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit {
  public readonly title = 'Money Tracker';
  public isLoading = true;
  public isLoginRoute = false;
  public isSavingPayment = false;
  public isSavingIncome = false;
  public isSavingForecastIncome = false;
  public isSavingForecastExpense = false;
  public isSavingCategory = false;
  public successMessage = '';
  public errorMessage = '';
  public theme: 'light' | 'dark' = 'dark';

  public categories: PaymentCategory[] = [];
  public payments: PaymentRow[] = [];
  public incomes: IncomeRow[] = [];
  public paymentOccurrences: ForecastOccurrenceRow[] = [];
  public incomeOccurrences: ForecastOccurrenceRow[] = [];
  public forecastRecurrenceRuleTypes: ForecastRecurrenceRuleTypeOption[] = [];
  public forecastIncomeDefinitions: ForecastIncomeDefinition[] = [];
  public forecastExpenseDefinitions: ForecastExpenseDefinition[] = [];
  public forecastIncomeRows: ForecastIncomeRow[] = [];
  public forecastExpenseRows: ForecastExpenseRow[] = [];

  public editingPaymentId: string | null = null;
  public editingIncomeId: string | null = null;
  public editingCategoryId: string | null = null;
  public editingForecastIncomeId: string | null = null;
  public editingForecastExpenseId: string | null = null;

  public paymentForm = this.createEmptyPaymentForm();
  public incomeForm = this.createEmptyIncomeForm();
  public categoryForm = this.createEmptyCategoryForm();
  public forecastIncomeForm = this.createEmptyForecastIncomeForm();
  public forecastExpenseForm = this.createEmptyForecastExpenseForm();

  private readonly paymentQuery: PaymentQuery = {
    month: this.toMonthInput(new Date()),
    pageNumber: 1,
    pageSize: 100,
    sortBy: 'Date',
    sortOrder: 'asc',
  };

  private readonly incomeQuery: IncomeQuery = {
    month: this.toMonthInput(new Date()),
    pageNumber: 1,
    pageSize: 100,
    sortBy: 'Date',
    sortOrder: 'asc',
  };

  public readonly paymentFilters = {
    month: this.paymentQuery.month,
    categoryId: '',
    descriptionFilter: '',
    minAmount: null as number | null,
    maxAmount: null as number | null,
  };

  public readonly incomeFilters = {
    month: this.incomeQuery.month,
    descriptionFilter: '',
    minAmount: null as number | null,
    maxAmount: null as number | null,
  };

  public readonly forecastRange = {
    startDate: this.toInputDate(new Date()),
    endDate: this.toInputDate(this.addDays(new Date(), 30)),
  };

  private readonly themeStorageKey = 'money-tracker.theme';

  public dialogState: {
    isVisible: boolean;
    options: {
      title?: string;
      message: string;
      confirmButtonText?: string;
      cancelButtonText?: string;
      isDangerous?: boolean;
    };
  } = {
    isVisible: false,
    options: {
      message: '',
    },
  };

  constructor(
    private readonly moneyTrackerApiService: MoneyTrackerApiService,
    private readonly ngZone: NgZone,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly confirmationDialogService: ConfirmationDialogService
  ) {}

  async ngOnInit(): Promise<void> {
    this.confirmationDialogService.getDialogState().subscribe((state) => {
      this.ngZone.run(() => {
        this.dialogState = state;
      });
    });

    this.initializeTheme();
    this.isLoginRoute = this.router.url.startsWith('/login');

    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd)
    ).subscribe(async (e: NavigationEnd) => {
      const wasLogin = this.isLoginRoute;
      this.isLoginRoute = e.urlAfterRedirects.startsWith('/login');

      if (wasLogin && !this.isLoginRoute) {
        await this.reloadDashboard();
      }
    });

    if (!this.isLoginRoute) {
      await this.reloadDashboard();
    }
  }

  public get isDarkTheme(): boolean {
    return this.theme === 'dark';
  }

  public toggleTheme(): void {
    this.setTheme(this.isDarkTheme ? 'light' : 'dark', true);
  }

  public async reloadDashboard(): Promise<void> {
    this.isLoading = true;
    this.clearMessages();

    const results = await Promise.allSettled([
      this.loadCategories(),
      this.loadPayments(),
      this.loadIncomes(),
      this.loadPaymentOccurrences(),
      this.loadIncomeOccurrences(),
      this.loadForecastRecurrenceRuleTypes(),
      this.loadForecastDefinitions(),
      this.loadForecastRows(),
    ]);

    const failures = results.filter(
      (result): result is PromiseRejectedResult => result.status === 'rejected'
    );

    this.ngZone.run(() => {
      if (failures.length > 0) {
        this.errorMessage = failures
          .map((failure) => this.getErrorMessage(failure.reason, 'Unable to load dashboard data.'))
          .join(' ');
      }

      this.isLoading = false;
    });
  }

  public async submitPayment(): Promise<void> {
    if (!this.paymentForm.description.trim() || !this.paymentForm.paymentCategoryId || !this.paymentForm.amount || !this.paymentForm.date) {
      this.errorMessage = 'Complete all required payment fields before saving.';
      this.successMessage = '';
      return;
    }

    this.isSavingPayment = true;
    this.clearMessages();

    const model: PaymentFormModel = {
      ...this.paymentForm,
      description: this.paymentForm.description.trim(),
      forecastOccurrenceId: this.paymentForm.forecastOccurrenceId || null,
    };

    try {
      if (this.editingPaymentId) {
        await this.moneyTrackerApiService.updatePayment(this.editingPaymentId, model);
        this.ngZone.run(() => {
          this.successMessage = 'Payment updated successfully.';
        });
      } else {
        await this.moneyTrackerApiService.createPayment(model);
        this.ngZone.run(() => {
          this.successMessage = 'Payment created successfully.';
        });
      }

      await Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
      this.ngZone.run(() => {
        this.resetPaymentForm();
      });
    } catch (error) {
      this.ngZone.run(() => {
        this.errorMessage = this.getErrorMessage(error, 'Unable to save the payment.');
      });
    } finally {
      this.ngZone.run(() => {
        this.isSavingPayment = false;
      });
    }
  }

  public startPaymentEdit(payment: PaymentRow): void {
    this.editingPaymentId = payment.id;
    this.paymentForm = {
      description: payment.description,
      paymentCategoryId: payment.paymentCategoryId,
      forecastOccurrenceId: payment.forecastOccurrenceId ?? null,
      amount: payment.amount,
      date: this.toInputDate(payment.date),
      isOneShot: payment.isOneShot,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public startPaymentFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.editingPaymentId = null;
    this.paymentForm = {
      description: occurrence.description,
      paymentCategoryId: occurrence.paymentCategoryId ?? '',
      forecastOccurrenceId: occurrence.id,
      amount: occurrence.amount,
      date: occurrence.expectedDate,
      isOneShot: true,
    };
    this.clearMessages();
  }

  public async discardPendingForecastOccurrence(occurrence: ForecastOccurrenceRow, isIncome: boolean): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Discard Forecast Occurrence',
      message: `Discard pending forecast occurrence "${occurrence.description}" scheduled on ${this.toDisplayDate(occurrence.expectedDate)}?`,
      confirmButtonText: 'Discard',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) {
      return;
    }

    this.clearMessages();

    try {
      if (isIncome) {
        await this.moneyTrackerApiService.discardForecastIncomeOccurrence(occurrence.id);
        await Promise.all([this.loadIncomeOccurrences(), this.loadIncomes(), this.loadForecastRows()]);

        if (this.incomeForm.forecastOccurrenceId === occurrence.id) {
          this.incomeForm.forecastOccurrenceId = null;
        }
      } else {
        await this.moneyTrackerApiService.discardForecastExpenseOccurrence(occurrence.id);
        await Promise.all([this.loadPaymentOccurrences(), this.loadPayments(), this.loadForecastRows()]);

        if (this.paymentForm.forecastOccurrenceId === occurrence.id) {
          this.paymentForm.forecastOccurrenceId = null;
        }
      }

      this.successMessage = 'Forecast occurrence discarded successfully.';
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to discard the forecast occurrence.');
    }
  }

  public clearPaymentForecastOccurrence(): void {
    this.paymentForm.forecastOccurrenceId = null;
  }

  public cancelPaymentEdit(): void {
    this.resetPaymentForm();
    this.clearMessages();
  }

  public async deletePayment(payment: PaymentRow): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Payment',
      message: `Delete payment "${payment.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) {
      return;
    }

    this.clearMessages();

    try {
      const occurrenceAction = this.resolveOccurrenceDeleteAction(payment.description, payment.forecastOccurrenceId, payment.forecastExpectedDate);
      await this.moneyTrackerApiService.deletePayment(payment.id, occurrenceAction);
      await Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
      this.successMessage = 'Payment deleted successfully.';

      if (this.editingPaymentId === payment.id) {
        this.resetPaymentForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the payment.');
    }
  }

  public async submitForecastIncome(): Promise<void> {
    const selectedRecurrenceTypeId = this.forecastIncomeForm.forecastRecurrenceRuleTypeId.trim();
    const isOneTime = this.isOneTimeRecurrenceTypeId(selectedRecurrenceTypeId);

    if (!selectedRecurrenceTypeId || !this.forecastIncomeForm.description.trim() || !this.forecastIncomeForm.amount || !this.forecastIncomeForm.recurrenceStart || (!isOneTime && this.forecastIncomeForm.interval < 1)) {
      this.errorMessage = 'Complete all required forecast income fields before saving.';
      this.successMessage = '';
      return;
    }

    this.isSavingForecastIncome = true;
    this.clearMessages();

    const model: ForecastIncomeFormModel = {
      ...this.forecastIncomeForm,
      forecastRecurrenceRuleTypeId: selectedRecurrenceTypeId,
      interval: isOneTime ? 1 : this.forecastIncomeForm.interval,
      description: this.forecastIncomeForm.description.trim(),
    };

    try {
      if (this.editingForecastIncomeId) {
        await this.moneyTrackerApiService.updateForecastIncomeDefinition(this.editingForecastIncomeId, model);
        this.successMessage = 'Forecast income updated successfully.';
      } else {
        await this.moneyTrackerApiService.createForecastIncomeDefinition(model);
        this.successMessage = 'Forecast income created successfully.';
      }

      await Promise.all([
        this.loadForecastDefinitions(),
        this.loadForecastRows(),
        this.loadIncomeOccurrences(),
      ]);
      this.resetForecastIncomeForm();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to save the forecast income.');
    } finally {
      this.isSavingForecastIncome = false;
    }
  }

  public async submitForecastExpense(): Promise<void> {
    const selectedRecurrenceTypeId = this.forecastExpenseForm.forecastRecurrenceRuleTypeId.trim();
    const isOneTime = this.isOneTimeRecurrenceTypeId(selectedRecurrenceTypeId);

    if (!selectedRecurrenceTypeId || !this.forecastExpenseForm.description.trim() || !this.forecastExpenseForm.amount || !this.forecastExpenseForm.recurrenceStart || (!isOneTime && this.forecastExpenseForm.interval < 1)) {
      this.errorMessage = 'Complete all required forecast expense fields before saving.';
      this.successMessage = '';
      return;
    }

    if (!this.forecastExpenseForm.paymentCategoryId) {
      this.errorMessage = 'Forecast expenses require a payment category.';
      this.successMessage = '';
      return;
    }

    this.isSavingForecastExpense = true;
    this.clearMessages();

    const model: ForecastExpenseFormModel = {
      ...this.forecastExpenseForm,
      forecastRecurrenceRuleTypeId: selectedRecurrenceTypeId,
      interval: isOneTime ? 1 : this.forecastExpenseForm.interval,
      description: this.forecastExpenseForm.description.trim(),
    };

    try {
      if (this.editingForecastExpenseId) {
        await this.moneyTrackerApiService.updateForecastExpenseDefinition(this.editingForecastExpenseId, model);
        this.successMessage = 'Forecast expense updated successfully.';
      } else {
        await this.moneyTrackerApiService.createForecastExpenseDefinition(model);
        this.successMessage = 'Forecast expense created successfully.';
      }

      await Promise.all([
        this.loadForecastDefinitions(),
        this.loadForecastRows(),
        this.loadPaymentOccurrences(),
      ]);
      this.resetForecastExpenseForm();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to save the forecast expense.');
    } finally {
      this.isSavingForecastExpense = false;
    }
  }

  public async submitCategory(): Promise<void> {
    const name = this.categoryForm.name.trim();
    const code = this.categoryForm.code.trim().toUpperCase();

    if (!name || !code) {
      this.errorMessage = 'Complete all required category fields before saving.';
      this.successMessage = '';
      return;
    }

    this.isSavingCategory = true;
    this.clearMessages();

    const model: PaymentCategoryFormModel = {
      name,
      code,
    };

    try {
      if (this.editingCategoryId) {
        await this.moneyTrackerApiService.updateCategory(this.editingCategoryId, model);
        this.successMessage = 'Category updated successfully.';
      } else {
        await this.moneyTrackerApiService.createCategory(model);
        this.successMessage = 'Category created successfully.';
      }

      await Promise.all([this.loadCategories(), this.loadPayments(), this.loadForecastDefinitions(), this.loadForecastRows(), this.loadPaymentOccurrences()]);
      this.resetCategoryForm();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to save the category.');
    } finally {
      this.isSavingCategory = false;
    }
  }

  public startCategoryEdit(category: PaymentCategory): void {
    this.editingCategoryId = category.id;
    this.categoryForm = {
      name: category.name,
      code: category.code,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public cancelCategoryEdit(): void {
    this.resetCategoryForm();
    this.clearMessages();
  }

  public async deleteCategory(category: PaymentCategory): Promise<void> {
    if (!confirm(`Delete category "${category.name}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deleteCategory(category.id);
      await Promise.all([this.loadCategories(), this.loadPayments(), this.loadForecastDefinitions(), this.loadForecastRows(), this.loadPaymentOccurrences()]);
      this.successMessage = 'Category deleted successfully.';

      if (this.editingCategoryId === category.id) {
        this.resetCategoryForm();
      }

      if (this.paymentForm.paymentCategoryId === category.id) {
        this.paymentForm.paymentCategoryId = '';
      }

      if (this.forecastExpenseForm.paymentCategoryId === category.id) {
        this.forecastExpenseForm.paymentCategoryId = '';
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the category.');
    }
  }

  public startForecastEdit(definition: ForecastIncomeDefinition): void {
    this.editingForecastIncomeId = definition.id;
    this.forecastIncomeForm = {
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.toInputDate(definition.recurrenceEnd) : '',
      interval: definition.interval,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public cancelForecastEdit(): void {
    this.resetForecastIncomeForm();
    this.clearMessages();
  }

  public async deleteForecast(definition: ForecastIncomeDefinition): Promise<void> {
    if (!confirm(`Delete forecast "${definition.description}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deleteForecastIncomeDefinition(definition.id);
      await Promise.all([
        this.loadForecastDefinitions(),
        this.loadForecastRows(),
        this.loadPaymentOccurrences(),
        this.loadIncomeOccurrences(),
      ]);
      this.successMessage = 'Forecast deleted successfully.';

      if (this.editingForecastIncomeId === definition.id) {
        this.resetForecastIncomeForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the forecast.');
    }
  }

  public getForecastRecurrenceRuleTypeLabel(forecastRecurrenceRuleTypeId: string): string {
    const type = this.forecastRecurrenceRuleTypes.find((x) => x.id === forecastRecurrenceRuleTypeId);
    return type ? `${type.name} (${type.code})` : forecastRecurrenceRuleTypeId;
  }
  public async submitIncome(): Promise<void> {
    if (!this.incomeForm.description.trim() || !this.incomeForm.amount || !this.incomeForm.date) {
      this.errorMessage = 'Complete all required income fields before saving.';
      this.successMessage = '';
      return;
    }

    this.isSavingIncome = true;
    this.clearMessages();

    const model: IncomeFormModel = {
      ...this.incomeForm,
      description: this.incomeForm.description.trim(),
      forecastOccurrenceId: this.incomeForm.forecastOccurrenceId || null,
    };

    try {
      if (this.editingIncomeId) {
        await this.moneyTrackerApiService.updateIncome(this.editingIncomeId, model);
        this.successMessage = 'Income updated successfully.';
      } else {
        await this.moneyTrackerApiService.createIncome(model);
        this.successMessage = 'Income created successfully.';
      }

      await Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
      this.resetIncomeForm();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to save the income.');
    } finally {
      this.isSavingIncome = false;
    }
  }

  public startIncomeEdit(income: IncomeRow): void {
    this.editingIncomeId = income.id;
    this.incomeForm = {
      description: income.description,
      forecastOccurrenceId: income.forecastOccurrenceId ?? null,
      amount: income.amount,
      date: this.toInputDate(income.date),
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public startIncomeFromOccurrence(occurrence: ForecastOccurrenceRow): void {
    this.editingIncomeId = null;
    this.incomeForm = {
      description: occurrence.description,
      forecastOccurrenceId: occurrence.id,
      amount: occurrence.amount,
      date: occurrence.expectedDate,
    };
    this.clearMessages();
  }

  public clearIncomeForecastOccurrence(): void {
    this.incomeForm.forecastOccurrenceId = null;
  }

  public cancelIncomeEdit(): void {
    this.resetIncomeForm();
    this.clearMessages();
  }

  public async deleteIncome(income: IncomeRow): Promise<void> {
    const confirmed = await this.confirmationDialogService.show({
      title: 'Delete Income',
      message: `Delete income "${income.description}"?`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      isDangerous: true,
    });

    if (!confirmed) {
      return;
    }

    this.clearMessages();

    try {
      const occurrenceAction = this.resolveOccurrenceDeleteAction(income.description, income.forecastOccurrenceId, income.forecastExpectedDate);
      await this.moneyTrackerApiService.deleteIncome(income.id, occurrenceAction);
      await Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
      this.successMessage = 'Income deleted successfully.';

      if (this.editingIncomeId === income.id) {
        this.resetIncomeForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the income.');
    }
  }

  public get paymentTotal(): number {
    return this.payments.reduce((total, payment) => total + Number(payment.amount ?? 0), 0);
  }

  public get incomeTotal(): number {
    return this.incomes.reduce((total, income) => total + Number(income.amount ?? 0), 0);
  }

  public get forecastIncomeTotal(): number {
    return this.forecastIncomeRows.reduce((total, row) => total + Number(row.amount ?? 0), 0);
  }

  public get forecastExpenseTotal(): number {
    return this.forecastExpenseRows.reduce((total, row) => total + Number(row.amount ?? 0), 0);
  }

  public get forecastBalance(): number {
    return this.forecastIncomeTotal - this.forecastExpenseTotal;
  }

  public get latestPayment(): PaymentRow | undefined {
    return this.payments[this.payments.length - 1];
  }


  public get nextForecast(): ForecastExpenseRow | undefined {
    return this.forecastExpenseRows[0];
  }

  public get hasCategories(): boolean {
    return this.categories.length > 0;
  }

  public trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  public resetPaymentForm(): void {
    this.editingPaymentId = null;
    this.paymentForm = this.createEmptyPaymentForm();
  }

  public resetIncomeForm(): void {
    this.editingIncomeId = null;
    this.incomeForm = this.createEmptyIncomeForm();
  }

  public resetCategoryForm(): void {
    this.editingCategoryId = null;
    this.categoryForm = this.createEmptyCategoryForm();
  }

  public resetForecastIncomeForm(): void {
    this.editingForecastIncomeId = null;
    this.forecastIncomeForm = this.createEmptyForecastIncomeForm();
  }

  public resetForecastExpenseForm(): void {
    this.editingForecastExpenseId = null;
    this.forecastExpenseForm = this.createEmptyForecastExpenseForm();
  }

  public onPaymentMonthChanged(month: string): void {
    this.paymentFilters.month = month;
    this.paymentQuery.month = month;
    void Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  public get selectedPaymentMonth(): string {
    return this.paymentFilters.month;
  }

  public applyPaymentFilters(): void {
    this.paymentQuery.month = this.paymentFilters.month;
    this.paymentQuery.categoryId = this.paymentFilters.categoryId || undefined;
    this.paymentQuery.descriptionFilter = this.paymentFilters.descriptionFilter.trim() || undefined;
    this.paymentQuery.minAmount = this.paymentFilters.minAmount ?? undefined;
    this.paymentQuery.maxAmount = this.paymentFilters.maxAmount ?? undefined;
    void Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  public onIncomeMonthChanged(month: string): void {
    this.incomeFilters.month = month;
    this.incomeQuery.month = month;
    void Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  public get selectedIncomeMonth(): string {
    return this.incomeFilters.month;
  }

  public applyIncomeFilters(): void {
    this.incomeQuery.month = this.incomeFilters.month;
    this.incomeQuery.descriptionFilter = this.incomeFilters.descriptionFilter.trim() || undefined;
    this.incomeQuery.minAmount = this.incomeFilters.minAmount ?? undefined;
    this.incomeQuery.maxAmount = this.incomeFilters.maxAmount ?? undefined;
    void Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  public resetPaymentFilters(): void {
    this.paymentFilters.month = this.toMonthInput(new Date());
    this.paymentFilters.categoryId = '';
    this.paymentFilters.descriptionFilter = '';
    this.paymentFilters.minAmount = null;
    this.paymentFilters.maxAmount = null;

    this.paymentQuery.month = this.paymentFilters.month;
    this.paymentQuery.categoryId = undefined;
    this.paymentQuery.descriptionFilter = undefined;
    this.paymentQuery.minAmount = undefined;
    this.paymentQuery.maxAmount = undefined;

    void Promise.all([this.loadPayments(), this.loadPaymentOccurrences()]);
  }

  public resetIncomeFilters(): void {
    this.incomeFilters.month = this.toMonthInput(new Date());
    this.incomeFilters.descriptionFilter = '';
    this.incomeFilters.minAmount = null;
    this.incomeFilters.maxAmount = null;

    this.incomeQuery.month = this.incomeFilters.month;
    this.incomeQuery.descriptionFilter = undefined;
    this.incomeQuery.minAmount = undefined;
    this.incomeQuery.maxAmount = undefined;

    void Promise.all([this.loadIncomes(), this.loadIncomeOccurrences()]);
  }

  private async loadCategories(): Promise<void> {
    const categories = await this.moneyTrackerApiService.getCategories();
    this.ngZone.run(() => {
      this.categories = [...categories].sort((left, right) => left.name.localeCompare(right.name));

      if (this.paymentForm.paymentCategoryId && !this.categories.some((x) => x.id === this.paymentForm.paymentCategoryId)) {
        this.paymentForm.paymentCategoryId = '';
      }

      if (this.forecastExpenseForm.paymentCategoryId && !this.categories.some((x) => x.id === this.forecastExpenseForm.paymentCategoryId)) {
        this.forecastExpenseForm.paymentCategoryId = '';
      }
    });
  }

  private async loadPayments(): Promise<void> {
    const pageSize = this.paymentQuery.pageSize ?? 100;
    const baseQuery: PaymentQuery = {
      ...this.paymentQuery,
      pageSize,
      pageNumber: 1,
    };

    const firstPage = await this.moneyTrackerApiService.getPayments(baseQuery);
    const allItems = [...firstPage.items];
    const totalPages = Math.max(1, Math.ceil(firstPage.totalItems / pageSize));

    for (let page = 2; page <= totalPages; page++) {
      const nextPage = await this.moneyTrackerApiService.getPayments({ ...baseQuery, pageNumber: page });
      allItems.push(...nextPage.items);
    }

    this.ngZone.run(() => {
      this.payments = [...allItems].sort((left, right) => left.date.localeCompare(right.date));
    });
  }

  private async loadIncomes(): Promise<void> {
    const pageSize = this.incomeQuery.pageSize ?? 100;
    const baseQuery: IncomeQuery = {
      ...this.incomeQuery,
      pageSize,
      pageNumber: 1,
    };

    const firstPage = await this.moneyTrackerApiService.getIncomes(baseQuery);
    const allItems = [...firstPage.items];
    const totalPages = Math.max(1, Math.ceil(firstPage.totalItems / pageSize));

    for (let page = 2; page <= totalPages; page++) {
      const nextPage = await this.moneyTrackerApiService.getIncomes({ ...baseQuery, pageNumber: page });
      allItems.push(...nextPage.items);
    }

    this.ngZone.run(() => {
      this.incomes = [...allItems].sort((left, right) => left.date.localeCompare(right.date));
    });
  }

  private async loadPaymentOccurrences(): Promise<void> {
    const occurrences = await this.moneyTrackerApiService.getForecastExpenseOccurrences(this.paymentQuery.month);
    this.ngZone.run(() => {
      this.paymentOccurrences = [...occurrences].sort((left, right) => left.expectedDate.localeCompare(right.expectedDate));
    });
  }

  private async loadIncomeOccurrences(): Promise<void> {
    const occurrences = await this.moneyTrackerApiService.getForecastIncomeOccurrences(this.incomeQuery.month);
    this.ngZone.run(() => {
      this.incomeOccurrences = [...occurrences].sort((left, right) => left.expectedDate.localeCompare(right.expectedDate));
    });
  }

  private async loadForecastRecurrenceRuleTypes(): Promise<void> {
    const types = await this.moneyTrackerApiService.getForecastRecurrenceRuleTypes();

    this.ngZone.run(() => {
      this.forecastRecurrenceRuleTypes = [...types].sort((left, right) => left.name.localeCompare(right.name));

      const oneTimeType = this.forecastRecurrenceRuleTypes.find((x) => x.code === 'O');
      const defaultTypeId = oneTimeType?.id ?? this.forecastRecurrenceRuleTypes[0]?.id ?? '';

      if (!this.forecastIncomeForm.forecastRecurrenceRuleTypeId) {
        this.forecastIncomeForm.forecastRecurrenceRuleTypeId = defaultTypeId;
        this.onForecastIncomeRecurrenceTypeChanged();
      }

      if (!this.forecastExpenseForm.forecastRecurrenceRuleTypeId) {
        this.forecastExpenseForm.forecastRecurrenceRuleTypeId = defaultTypeId;
        this.onForecastExpenseRecurrenceTypeChanged();
      }
    });
  }

  private async loadForecastDefinitions(): Promise<void> {
    const [incomeDefinitions, expenseDefinitions] = await Promise.all([
      this.moneyTrackerApiService.getForecastIncomeDefinitions(),
      this.moneyTrackerApiService.getForecastExpenseDefinitions(),
    ]);

    this.ngZone.run(() => {
      this.forecastIncomeDefinitions = [...incomeDefinitions].sort((left, right) => {
        const dateComparison = left.recurrenceStart.localeCompare(right.recurrenceStart);
        return dateComparison !== 0 ? dateComparison : left.description.localeCompare(right.description);
      });

      this.forecastExpenseDefinitions = [...expenseDefinitions].sort((left, right) => {
        const dateComparison = left.recurrenceStart.localeCompare(right.recurrenceStart);
        return dateComparison !== 0 ? dateComparison : left.description.localeCompare(right.description);
      });
    });
  }

  private async loadForecastRows(): Promise<void> {
    const [incomeRows, expenseRows] = await Promise.all([
      this.moneyTrackerApiService.getForecastIncomeRows(this.forecastRange.startDate, this.forecastRange.endDate),
      this.moneyTrackerApiService.getForecastExpenseRows(this.forecastRange.startDate, this.forecastRange.endDate),
    ]);

    this.ngZone.run(() => {
      this.forecastIncomeRows = [...incomeRows].sort((left, right) => left.date.localeCompare(right.date));
      this.forecastExpenseRows = [...expenseRows].sort((left, right) => left.date.localeCompare(right.date));
    });
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }

  private createEmptyPaymentForm(): PaymentFormModel {
    return {
      description: '',
      paymentCategoryId: '',
      forecastOccurrenceId: null,
      amount: null,
      date: this.toInputDate(new Date()),
      isOneShot: true,
    };
  }

  private createEmptyIncomeForm(): IncomeFormModel {
    return {
      description: '',
      forecastOccurrenceId: null,
      amount: null,
      date: this.toInputDate(new Date()),
    };
  }

  private createEmptyCategoryForm(): PaymentCategoryFormModel {
    return {
      name: '',
      code: '',
    };
  }

  private createEmptyForecastIncomeForm(): ForecastIncomeFormModel {
    return {
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: this.toInputDate(new Date()),
      recurrenceEnd: '',
      interval: 1,
    };
  }

  private createEmptyForecastExpenseForm(): ForecastExpenseFormModel {
    return {
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: this.toInputDate(new Date()),
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: '',
    };
  }

  private isOneTimeRecurrenceTypeId(forecastRecurrenceRuleTypeId: string): boolean {
    const type = this.forecastRecurrenceRuleTypes.find((x) => x.id === forecastRecurrenceRuleTypeId);
    return type?.code === 'O';
  }

  private resolveOccurrenceDeleteAction(description: string, forecastOccurrenceId?: string | null, forecastExpectedDate?: string | null): OccurrenceDeleteAction | undefined {
    if (!forecastOccurrenceId || !forecastExpectedDate || !this.isPastDate(forecastExpectedDate)) {
      return undefined;
    }

    const reopen = confirm(
      `"${description}" is linked to a past forecast occurrence scheduled on ${this.toDisplayDate(forecastExpectedDate)}.\n\nSelect OK to move it back to pending.\nSelect Cancel to skip the occurrence.`
    );

    return reopen ? 'Reopen' : 'Skip';
  }

  private isPastDate(value: string): boolean {
    const parsed = new Date(`${value.slice(0, 10)}T00:00:00`);
    if (Number.isNaN(parsed.getTime())) {
      return false;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return parsed < today;
  }

  private toDisplayDate(value: string): string {
    const parsed = new Date(`${value.slice(0, 10)}T00:00:00`);
    return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleDateString();
  }

  private addDays(date: Date, days: number): Date {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  }

  private toInputDate(value: Date | string): string {
    if (typeof value === 'string') {
      return value.slice(0, 10);
    }

    const timezoneOffset = value.getTimezoneOffset() * 60000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 10);
  }

  private toMonthInput(value: Date): string {
    const timezoneOffset = value.getTimezoneOffset() * 60000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 7);
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof Error && error.message) {
      return error.message;
    }

    return fallbackMessage;
  }

  public logout(): void {
    this.authService.logout();
  }

  private initializeTheme(): void {
    const preferredTheme = this.getPreferredTheme();
    this.setTheme(preferredTheme, false);
  }

  private getPreferredTheme(): 'light' | 'dark' {
    try {
      const stored = localStorage.getItem(this.themeStorageKey);
      if (stored === 'light' || stored === 'dark') {
        return stored;
      }
    } catch (error) {
      void error;
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private setTheme(theme: 'light' | 'dark', persist: boolean): void {
    this.theme = theme;
    document.documentElement.setAttribute('data-theme', theme);

    if (persist) {
      try {
        localStorage.setItem(this.themeStorageKey, theme);
      } catch (error) {
        void error;
      }
    }
  }

  public onDialogConfirmed(): void {
    this.confirmationDialogService.confirm();
  }

  public onDialogCancelled(): void {
    this.confirmationDialogService.cancel();
  }

  public startForecastIncomeEdit(definition: ForecastIncomeDefinition): void {
    this.editingForecastIncomeId = definition.id;
    this.forecastIncomeForm = {
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.toInputDate(definition.recurrenceEnd) : '',
      interval: definition.interval,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public startForecastExpenseEdit(definition: ForecastExpenseDefinition): void {
    this.editingForecastExpenseId = definition.id;
    this.forecastExpenseForm = {
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.toInputDate(definition.recurrenceEnd) : '',
      interval: definition.interval,
      paymentCategoryId: definition.paymentCategoryId ?? '',
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public cancelForecastIncomeEdit(): void {
    this.resetForecastIncomeForm();
    this.clearMessages();
  }

  public cancelForecastExpenseEdit(): void {
    this.resetForecastExpenseForm();
    this.clearMessages();
  }

  public async deleteForecastIncome(definition: ForecastIncomeDefinition): Promise<void> {
    if (!confirm(`Delete forecast income "${definition.description}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deleteForecastIncomeDefinition(definition.id);
      await Promise.all([
        this.loadForecastDefinitions(),
        this.loadForecastRows(),
        this.loadIncomeOccurrences(),
      ]);
      this.successMessage = 'Forecast income deleted successfully.';

      if (this.editingForecastIncomeId === definition.id) {
        this.resetForecastIncomeForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the forecast income.');
    }
  }

  public async deleteForecastExpense(definition: ForecastExpenseDefinition): Promise<void> {
    if (!confirm(`Delete forecast expense "${definition.description}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deleteForecastExpenseDefinition(definition.id);
      await Promise.all([
        this.loadForecastDefinitions(),
        this.loadForecastRows(),
        this.loadPaymentOccurrences(),
      ]);
      this.successMessage = 'Forecast expense deleted successfully.';

      if (this.editingForecastExpenseId === definition.id) {
        this.resetForecastExpenseForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the forecast expense.');
    }
  }

  public isOneTimeForecastIncomeRecurrenceTypeSelected(): boolean {
    return this.isOneTimeRecurrenceTypeId(this.forecastIncomeForm.forecastRecurrenceRuleTypeId);
  }

  public isOneTimeForecastExpenseRecurrenceTypeSelected(): boolean {
    return this.isOneTimeRecurrenceTypeId(this.forecastExpenseForm.forecastRecurrenceRuleTypeId);
  }

  public onForecastIncomeRecurrenceTypeChanged(): void {
    if (this.isOneTimeForecastIncomeRecurrenceTypeSelected()) {
      this.forecastIncomeForm.interval = 1;
    }
  }

  public onForecastExpenseRecurrenceTypeChanged(): void {
    if (this.isOneTimeForecastExpenseRecurrenceTypeSelected()) {
      this.forecastExpenseForm.interval = 1;
    }
  }

  public getForecastIncomeRecurrenceSummary(definition: ForecastIncomeDefinition): string {
    return this.getRecurrenceSummaryByType(definition.forecastRecurrenceRuleTypeId, definition.interval);
  }

  public getForecastExpenseRecurrenceSummary(definition: ForecastExpenseDefinition): string {
    return this.getRecurrenceSummaryByType(definition.forecastRecurrenceRuleTypeId, definition.interval);
  }

  public get upcomingForecastIncomes(): ForecastIncomeRow[] {
    return this.forecastIncomeRows.slice(0, 5);
  }

  public get upcomingForecastExpenses(): ForecastExpenseRow[] {
    return this.forecastExpenseRows.slice(0, 5);
  }

  private getRecurrenceSummaryByType(forecastRecurrenceRuleTypeId: string, interval: number): string {
    const type = this.forecastRecurrenceRuleTypes.find((x) => x.id === forecastRecurrenceRuleTypeId);
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
}
