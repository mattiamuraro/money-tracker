export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaymentCategory {
  id: string;
  name: string;
  code: string;
  createdAt: string;
  createdBy: string;
  modifiedAt: string;
  modifiedBy: string;
}

export interface PaymentCategoryFormModel {
  name: string;
  code: string;
}

export interface PaymentRow {
  id: string;
  description: string;
  paymentCategoryId: string;
  category: string;
  forecastOccurrenceId?: string | null;
  forecastExpectedDate?: string | null;
  amount: number;
  date: string;
  isOneShot: boolean;
}

export interface IncomeRow {
  id: string;
  description: string;
  forecastOccurrenceId?: string | null;
  forecastExpectedDate?: string | null;
  amount: number;
  date: string;
}

export interface ForecastRow {
  id: string;
  forecastDefinitionId: string;
  description: string;
  amount: number;
  date: string;
  isIncome: boolean;
  paymentCategoryId?: string | null;
  category?: string | null;
}

export interface ForecastOccurrenceRow {
  id: string;
  forecastDefinitionId: string;
  description: string;
  amount: number;
  expectedDate: string;
  isIncome: boolean;
  paymentCategoryId?: string | null;
  category?: string | null;
}

export interface ForecastDefinition {
  id: string;
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number;
  recurrenceStart: string;
  recurrenceEnd: string | null;
  interval: number;
  isIncome: boolean;
  paymentCategoryId?: string | null;
  category?: string | null;
}

export interface PaymentFormModel {
  description: string;
  paymentCategoryId: string;
  forecastOccurrenceId?: string | null;
  amount: number | null;
  date: string;
  isOneShot: boolean;
}

export interface IncomeFormModel {
  description: string;
  forecastOccurrenceId?: string | null;
  amount: number | null;
  date: string;
}

export interface ForecastFormModel {
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number | null;
  recurrenceStart: string;
  recurrenceEnd: string;
  interval: number;
  isIncome: boolean;
  paymentCategoryId: string;
}

export type OccurrenceDeleteAction = 'Auto' | 'Reopen' | 'Skip';

export interface ForecastRecurrenceRuleTypeOption {
  id: string;
  name: string;
  code: string;
}

export interface PaymentQuery {
  month: string;
  categoryId?: string;
  descriptionFilter?: string;
  minAmount?: number;
  maxAmount?: number;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface IncomeQuery {
  month: string;
  descriptionFilter?: string;
  minAmount?: number;
  maxAmount?: number;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}
