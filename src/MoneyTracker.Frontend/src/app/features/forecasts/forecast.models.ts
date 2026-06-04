export type OccurrenceDeleteAction = 'Auto' | 'Reopen' | 'Skip';

export interface ForecastRecurrenceRuleTypeOption {
  id: string;
  name: string;
  code: string;
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
