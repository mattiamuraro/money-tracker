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

export interface ForecastIncomeRow {
  id: string;
  forecastDefinitionId: string;
  description: string;
  amount: number;
  date: string;
}

export interface ForecastExpenseRow {
  id: string;
  forecastDefinitionId: string;
  description: string;
  amount: number;
  date: string;
  paymentCategoryId?: string | null;
  category?: string | null;
}

export interface ForecastIncomeDefinition {
  id: string;
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number;
  recurrenceStart: string;
  recurrenceEnd: string | null;
  interval: number;
}

export interface ForecastExpenseDefinition {
  id: string;
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number;
  recurrenceStart: string;
  recurrenceEnd: string | null;
  interval: number;
  paymentCategoryId?: string | null;
  category?: string | null;
}

export interface ForecastIncomeFormModel {
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number | null;
  recurrenceStart: string;
  recurrenceEnd: string;
  interval: number;
}

export interface ForecastExpenseFormModel {
  forecastRecurrenceRuleTypeId: string;
  description: string;
  amount: number | null;
  recurrenceStart: string;
  recurrenceEnd: string;
  interval: number;
  paymentCategoryId: string;
}
