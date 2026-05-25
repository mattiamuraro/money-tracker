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

export interface PaymentFormModel {
  description: string;
  paymentCategoryId: string;
  forecastOccurrenceId?: string | null;
  amount: number | null;
  date: string;
  isOneShot: boolean;
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
