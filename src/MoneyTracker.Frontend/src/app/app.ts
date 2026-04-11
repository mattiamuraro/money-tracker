import { Component, NgZone, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './auth/auth.service';
import { MoneyTrackerApiService } from './money-tracker-api.service';
import {
  ForecastDefinition,
  ForecastFormModel,
  ForecastRecurrenceRuleTypeOption,
  ForecastRow,
  PaymentCategory,
  PaymentFormModel,
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
  public isSavingForecast = false;
  public successMessage = '';
  public errorMessage = '';

  public categories: PaymentCategory[] = [];
  public payments: PaymentRow[] = [];
  public forecastRecurrenceRuleTypes: ForecastRecurrenceRuleTypeOption[] = [];
  public forecastDefinitions: ForecastDefinition[] = [];
  public forecastRows: ForecastRow[] = [];

  public editingPaymentId: string | null = null;
  public editingForecastId: string | null = null;

  public paymentForm = this.createEmptyPaymentForm();
  public forecastForm = this.createEmptyForecastForm();

  private readonly paymentQuery = {
    pageNumber: 1,
    pageSize: 12,
    sortBy: 'Date',
    sortOrder: 'desc',
  };

  public readonly forecastRange = {
    startDate: this.toInputDate(new Date()),
    endDate: this.toInputDate(this.addDays(new Date(), 30)),
  };

  constructor(
    private readonly moneyTrackerApiService: MoneyTrackerApiService,
    private readonly ngZone: NgZone,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  async ngOnInit(): Promise<void> {
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

  public async reloadDashboard(): Promise<void> {
    this.isLoading = true;
    this.clearMessages();

    const results = await Promise.allSettled([
      this.loadCategories(),
      this.loadPayments(),
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

      await this.loadPayments();
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
      amount: payment.amount,
      date: this.toInputDate(payment.date),
      isOneShot: payment.isOneShot,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public cancelPaymentEdit(): void {
    this.resetPaymentForm();
    this.clearMessages();
  }

  public async deletePayment(payment: PaymentRow): Promise<void> {
    if (!confirm(`Delete payment "${payment.description}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deletePayment(payment.id);
      await this.loadPayments();
      this.successMessage = 'Payment deleted successfully.';

      if (this.editingPaymentId === payment.id) {
        this.resetPaymentForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the payment.');
    }
  }

  public async submitForecast(): Promise<void> {
    const selectedRecurrenceTypeId = this.forecastForm.forecastRecurrenceRuleTypeId.trim();
    const isOneTime = this.isOneTimeRecurrenceTypeId(selectedRecurrenceTypeId);

    if (!selectedRecurrenceTypeId || !this.forecastForm.description.trim() || !this.forecastForm.amount || !this.forecastForm.recurrenceStart || (!isOneTime && this.forecastForm.dayInterval < 1)) {
      this.errorMessage = 'Complete all required forecast fields before saving.';
      this.successMessage = '';
      return;
    }

    this.isSavingForecast = true;
    this.clearMessages();

    const model: ForecastFormModel = {
      ...this.forecastForm,
      forecastRecurrenceRuleTypeId: selectedRecurrenceTypeId,
      dayInterval: isOneTime ? 1 : this.forecastForm.dayInterval,
      description: this.forecastForm.description.trim(),
    };

    try {
      if (this.editingForecastId) {
        await this.moneyTrackerApiService.updateForecastDefinition(this.editingForecastId, model);
        this.successMessage = 'Forecast updated successfully.';
      } else {
        await this.moneyTrackerApiService.createForecastDefinition(model);
        this.successMessage = 'Forecast created successfully.';
      }

      await Promise.all([this.loadForecastDefinitions(), this.loadForecastRows()]);
      this.resetForecastForm();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to save the forecast.');
    } finally {
      this.isSavingForecast = false;
    }
  }

  public startForecastEdit(definition: ForecastDefinition): void {
    this.editingForecastId = definition.id;
    this.forecastForm = {
      forecastRecurrenceRuleTypeId: definition.forecastRecurrenceRuleTypeId,
      description: definition.description,
      amount: definition.amount,
      recurrenceStart: this.toInputDate(definition.recurrenceStart),
      recurrenceEnd: definition.recurrenceEnd ? this.toInputDate(definition.recurrenceEnd) : '',
      dayInterval: definition.dayInterval,
      isIncome: definition.isIncome,
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  public cancelForecastEdit(): void {
    this.resetForecastForm();
    this.clearMessages();
  }

  public async deleteForecast(definition: ForecastDefinition): Promise<void> {
    if (!confirm(`Delete forecast "${definition.description}"?`)) {
      return;
    }

    this.clearMessages();

    try {
      await this.moneyTrackerApiService.deleteForecastDefinition(definition.id);
      await Promise.all([this.loadForecastDefinitions(), this.loadForecastRows()]);
      this.successMessage = 'Forecast deleted successfully.';

      if (this.editingForecastId === definition.id) {
        this.resetForecastForm();
      }
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error, 'Unable to delete the forecast.');
    }
  }

  public getForecastRecurrenceRuleTypeLabel(forecastRecurrenceRuleTypeId: string): string {
    const type = this.forecastRecurrenceRuleTypes.find((x) => x.id === forecastRecurrenceRuleTypeId);
    return type ? `${type.name} (${type.code})` : forecastRecurrenceRuleTypeId;
  }

  public isOneTimeRecurrenceTypeSelected(): boolean {
    return this.isOneTimeRecurrenceTypeId(this.forecastForm.forecastRecurrenceRuleTypeId);
  }

  public onForecastRecurrenceTypeChanged(): void {
    if (this.isOneTimeRecurrenceTypeSelected()) {
      this.forecastForm.dayInterval = 1;
    }
  }

  public getRecurrenceSummary(definition: ForecastDefinition): string {
    return this.isOneTimeRecurrenceTypeId(definition.forecastRecurrenceRuleTypeId)
      ? 'One time'
      : `Every ${definition.dayInterval} day${definition.dayInterval > 1 ? 's' : ''}`;
  }

  public get paymentTotal(): number {
    return this.payments.reduce((total, payment) => total + Number(payment.amount ?? 0), 0);
  }

  public get forecastIncomeTotal(): number {
    return this.forecastRows
      .filter((row) => row.isIncome)
      .reduce((total, row) => total + Number(row.amount ?? 0), 0);
  }

  public get forecastExpenseTotal(): number {
    return this.forecastRows
      .filter((row) => !row.isIncome)
      .reduce((total, row) => total + Number(row.amount ?? 0), 0);
  }

  public get forecastBalance(): number {
    return this.forecastIncomeTotal - this.forecastExpenseTotal;
  }

  public get latestPayment(): PaymentRow | undefined {
    return this.payments[0];
  }

  public get nextForecast(): ForecastRow | undefined {
    return this.forecastRows[0];
  }

  public get upcomingForecasts(): ForecastRow[] {
    return this.forecastRows.slice(0, 5);
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

  public resetForecastForm(): void {
    this.editingForecastId = null;
    this.forecastForm = this.createEmptyForecastForm();
  }

  private async loadCategories(): Promise<void> {
    const categories = await this.moneyTrackerApiService.getCategories();
    this.ngZone.run(() => {
      this.categories = [...categories].sort((left, right) => left.name.localeCompare(right.name));
    });
  }

  private async loadPayments(): Promise<void> {
    const response = await this.moneyTrackerApiService.getPayments(this.paymentQuery);
    this.ngZone.run(() => {
      this.payments = [...response.items].sort((left, right) => right.date.localeCompare(left.date));
    });
  }

  private async loadForecastRecurrenceRuleTypes(): Promise<void> {
    const types = await this.moneyTrackerApiService.getForecastRecurrenceRuleTypes();

    this.ngZone.run(() => {
      this.forecastRecurrenceRuleTypes = [...types].sort((left, right) => left.name.localeCompare(right.name));

      if (!this.forecastForm.forecastRecurrenceRuleTypeId) {
        const oneTimeType = this.forecastRecurrenceRuleTypes.find((x) => x.code === 'O');
        this.forecastForm.forecastRecurrenceRuleTypeId = oneTimeType?.id ?? this.forecastRecurrenceRuleTypes[0]?.id ?? '';
        this.onForecastRecurrenceTypeChanged();
      }
    });
  }

  private async loadForecastDefinitions(): Promise<void> {
    const definitions = await this.moneyTrackerApiService.getForecastDefinitions();
    this.ngZone.run(() => {
      this.forecastDefinitions = [...definitions].sort((left, right) => {
        const dateComparison = left.recurrenceStart.localeCompare(right.recurrenceStart);
        return dateComparison !== 0 ? dateComparison : left.description.localeCompare(right.description);
      });
    });
  }

  private async loadForecastRows(): Promise<void> {
    const rows = await this.moneyTrackerApiService.getForecastRows(
      this.forecastRange.startDate,
      this.forecastRange.endDate
    );

    this.ngZone.run(() => {
      this.forecastRows = [...rows].sort((left, right) => left.date.localeCompare(right.date));
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
      amount: null,
      date: this.toInputDate(new Date()),
      isOneShot: true,
    };
  }

  private createEmptyForecastForm(): ForecastFormModel {
    return {
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: this.toInputDate(new Date()),
      recurrenceEnd: '',
      dayInterval: 1,
      isIncome: false,
    };
  }

  private isOneTimeRecurrenceTypeId(forecastRecurrenceRuleTypeId: string): boolean {
    const type = this.forecastRecurrenceRuleTypes.find((x) => x.id === forecastRecurrenceRuleTypeId);
    return type?.code === 'O';
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

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof Error && error.message) {
      return error.message;
    }

    return fallbackMessage;
  }

  public logout(): void {
    this.authService.logout();
  }
}
