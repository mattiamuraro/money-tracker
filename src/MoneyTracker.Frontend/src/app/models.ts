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

export interface PaymentRow {
  id: string;
  description: string;
  paymentCategoryId: string;
  category: string;
  amount: number;
  date: string;
  isOneShot: boolean;
}

export interface ForecastRow {
  id: string;
  description: string;
  amount: number;
  date: string;
  isIncome: boolean;
}

export interface ForecastDefinition {
  id: string;
  description: string;
  amount: number;
  recurrenceStart: string;
  recurrenceEnd: string | null;
  dayInterval: number;
  isIncome: boolean;
}

export interface PaymentFormModel {
  description: string;
  paymentCategoryId: string;
  amount: number | null;
  date: string;
  isOneShot: boolean;
}

export interface ForecastFormModel {
  description: string;
  amount: number | null;
  recurrenceStart: string;
  recurrenceEnd: string;
  dayInterval: number;
  isIncome: boolean;
}

export interface PaymentQuery {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  startDate?: string;
  endDate?: string;
  categoryId?: string;
}
