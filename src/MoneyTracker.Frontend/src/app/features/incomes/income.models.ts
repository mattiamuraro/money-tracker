export interface IncomeRow {
  id: string;
  description: string;
  forecastOccurrenceId?: string | null;
  forecastExpectedDate?: string | null;
  amount: number;
  date: string;
}

export interface IncomeFormModel {
  description: string;
  forecastOccurrenceId?: string | null;
  amount: number | null;
  date: string;
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
