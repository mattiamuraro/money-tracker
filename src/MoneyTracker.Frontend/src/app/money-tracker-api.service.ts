import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  ForecastDefinition,
  ForecastFormModel,
  ForecastRecurrenceRuleTypeOption,
  ForecastRow,
  IncomeFormModel,
  IncomeQuery,
  IncomeRow,
  PaginatedResponse,
  PaymentCategory,
  PaymentCategoryFormModel,
  PaymentFormModel,
  PaymentQuery,
  PaymentRow,
} from './models';

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

  deletePayment(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/payments/${id}`));
  }

  deleteIncome(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/incomes/${id}`));
  }

  getForecastRows(startDate: string, endDate: string): Promise<ForecastRow[]> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return firstValueFrom(this.httpClient.get<ForecastRow[]>(`${this.apiBaseUrl}/forecasts`, { params }));
  }

  getForecastRecurrenceRuleTypes(): Promise<ForecastRecurrenceRuleTypeOption[]> {
    return firstValueFrom(
      this.httpClient.get<ForecastRecurrenceRuleTypeOption[]>(`${this.apiBaseUrl}/forecasts/recurrence-rule-types`)
    );
  }

  getForecastDefinitions(): Promise<ForecastDefinition[]> {
    return firstValueFrom(
      this.httpClient.get<ForecastDefinition[]>(`${this.apiBaseUrl}/forecasts/definitions`)
    );
  }

  createForecastDefinition(model: ForecastFormModel): Promise<string> {
    return firstValueFrom(
      this.httpClient.post<string>(`${this.apiBaseUrl}/forecasts/definitions`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
        isIncome: model.isIncome,
      })
    );
  }

  updateForecastDefinition(id: string, model: ForecastFormModel): Promise<void> {
    return firstValueFrom(
      this.httpClient.put<void>(`${this.apiBaseUrl}/forecasts/definitions/${id}`, {
        forecastRecurrenceRuleTypeId: model.forecastRecurrenceRuleTypeId,
        description: model.description,
        amount: model.amount,
        recurrenceStart: model.recurrenceStart,
        recurrenceEnd: model.recurrenceEnd || null,
        interval: model.interval,
        isIncome: model.isIncome,
      })
    );
  }

  deleteForecastDefinition(id: string): Promise<void> {
    return firstValueFrom(this.httpClient.delete<void>(`${this.apiBaseUrl}/forecasts/definitions/${id}`));
  }
}
