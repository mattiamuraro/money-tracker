import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  ForecastExpenseDefinition,
  ForecastExpenseFormModel,
  ForecastExpenseRow,
  ForecastIncomeDefinition,
  ForecastIncomeFormModel,
  ForecastIncomeRow,
  ForecastOccurrenceRow,
  ForecastRecurrenceRuleTypeOption,
  OccurrenceDeleteAction,
} from './features/forecasts/forecast.models';
import { IncomeFormModel, IncomeQuery, IncomeRow } from './features/incomes/income.models';
import { PaginatedResponse } from './shared/models/pagination.models';
import {
  PaymentCategory,
  PaymentCategoryFormModel,
  PaymentFormModel,
  PaymentQuery,
  PaymentRow,
} from './features/payments/payment.models';

export interface DashboardSummary {
  paymentsCount: number;
  paymentTotal: number;
  latestPaymentDescription: string | null;
  latestPaymentDate: string | null;
  forecastIncomeTotal: number;
  forecastExpenseTotal: number;
  forecastBalance: number;
  nextUpcomingExpenseDescription: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class MoneyTrackerApiService {
  private readonly apiBaseUrl = '/api/v1';

  constructor(private readonly httpClient: HttpClient) {}

  getCategories(): Promise<PaymentCategory[]> {
    return firstValueFrom(this.httpClient.get<PaymentCategory[]>(`${this.apiBaseUrl}/categories`));
  }

  createCategory(model: PaymentCategoryFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/categories`, {
        name: model.name,
        code: model.code,
      })
    );
  }

  updateCategory(id: string, model: PaymentCategoryFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/categories/${id}`, {
        name: model.name,
        code: model.code,
      })
    );
  }

  deleteCategory(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/categories/${id}`));
  }

  getPayments(query: PaymentQuery): Promise<PaginatedResponse<PaymentRow>> {
    let params = new HttpParams();

    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }

    return firstValueFrom(
      this.httpClient.get<PaginatedResponse<PaymentRow>>(`${this.apiBaseUrl}/payments`, { params })
    );
  }

  getIncomes(query: IncomeQuery): Promise<PaginatedResponse<IncomeRow>> {
    let params = new HttpParams();

    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }

    return firstValueFrom(
      this.httpClient.get<PaginatedResponse<IncomeRow>>(`${this.apiBaseUrl}/incomes`, { params })
    );
  }

  createPayment(model: PaymentFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/payments`, {
        description: model.description,
        paymentCategoryId: model.paymentCategoryId,
        forecastOccurrenceId: model.forecastOccurrenceId || null,
        amount: model.amount,
        date: model.date,
        isOneShot: model.isOneShot,
        createdBy: 'MoneyTracker.Frontend',
      })
    );
  }

  createIncome(model: IncomeFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/incomes`, {
        description: model.description,
        forecastOccurrenceId: model.forecastOccurrenceId || null,
        amount: model.amount,
        date: model.date,
      })
    );
  }

  updatePayment(id: string, model: PaymentFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/payments/${id}`, {
        description: model.description,
        paymentCategoryId: model.paymentCategoryId,
        amount: model.amount,
        date: model.date,
        isOneShot: model.isOneShot,
        modifiedBy: 'MoneyTracker.Frontend',
      })
    );
  }

  updateIncome(id: string, model: IncomeFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/incomes/${id}`, {
        description: model.description,
        amount: model.amount,
        date: model.date,
      })
    );
  }

  deletePayment(id: string, occurrenceAction?: OccurrenceDeleteAction): Promise<void> {
    const params = occurrenceAction ? new HttpParams().set('occurrenceAction', occurrenceAction) : undefined;
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/payments/${id}`, { params }));
  }

  deleteIncome(id: string, occurrenceAction?: OccurrenceDeleteAction): Promise<void> {
    const params = occurrenceAction ? new HttpParams().set('occurrenceAction', occurrenceAction) : undefined;
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/incomes/${id}`, { params }));
  }

  getForecastIncomeRows(startDate: string, endDate: string): Promise<ForecastIncomeRow[]> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return firstValueFrom(this.httpClient.get<ForecastIncomeRow[]>(`${this.apiBaseUrl}/forecast-incomes`, { params }));
  }

  getForecastExpenseRows(startDate: string, endDate: string): Promise<ForecastExpenseRow[]> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return firstValueFrom(this.httpClient.get<ForecastExpenseRow[]>(`${this.apiBaseUrl}/forecast-expenses`, { params }));
  }

  getDashboardSummary(month: string): Promise<DashboardSummary> {
    const params = new HttpParams().set('month', month);
    return firstValueFrom(
      this.httpClient.get<DashboardSummary>(`${this.apiBaseUrl}/dashboard/summary`, { params })
    );
  }

  getForecastIncomeOccurrences(month: string): Promise<ForecastOccurrenceRow[]> {
    const params = new HttpParams().set('month', month);
    return firstValueFrom(this.httpClient.get<ForecastOccurrenceRow[]>(`${this.apiBaseUrl}/forecast-incomes/occurrences`, { params }));
  }

  getForecastExpenseOccurrences(month: string): Promise<ForecastOccurrenceRow[]> {
    const params = new HttpParams().set('month', month);
    return firstValueFrom(this.httpClient.get<ForecastOccurrenceRow[]>(`${this.apiBaseUrl}/forecast-expenses/occurrences`, { params }));
  }

  discardForecastIncomeOccurrence(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/forecast-incomes/occurrences/${id}`));
  }

  discardForecastExpenseOccurrence(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/forecast-expenses/occurrences/${id}`));
  }

  getForecastRecurrenceRuleTypes(): Promise<ForecastRecurrenceRuleTypeOption[]> {
    return firstValueFrom(
      this.httpClient.get<ForecastRecurrenceRuleTypeOption[]>(`${this.apiBaseUrl}/forecast-recurrence-rule-types`)
    );
  }

  getForecastIncomeDefinitions(): Promise<ForecastIncomeDefinition[]> {
    return firstValueFrom(
      this.httpClient.get<ForecastIncomeDefinition[]>(`${this.apiBaseUrl}/forecast-incomes/definitions`)
    );
  }

  getForecastExpenseDefinitions(): Promise<ForecastExpenseDefinition[]> {
    return firstValueFrom(
      this.httpClient.get<ForecastExpenseDefinition[]>(`${this.apiBaseUrl}/forecast-expenses/definitions`)
    );
  }

  createForecastIncomeDefinition(model: ForecastIncomeFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/forecast-incomes/definitions`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
      })
    );
  }

  createForecastExpenseDefinition(model: ForecastExpenseFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/forecast-expenses/definitions`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
        paymentCategoryId: model.paymentCategoryId || null,
      })
    );
  }

  updateForecastIncomeDefinition(id: string, model: ForecastIncomeFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/forecast-incomes/definitions/${id}`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
      })
    );
  }

  updateForecastExpenseDefinition(id: string, model: ForecastExpenseFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/forecast-expenses/definitions/${id}`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
        paymentCategoryId: model.paymentCategoryId || null,
      })
    );
  }

  deleteForecastIncomeDefinition(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/forecast-incomes/definitions/${id}`));
  }

  deleteForecastExpenseDefinition(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/forecast-expenses/definitions/${id}`));
  }
}
