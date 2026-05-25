import { Injectable } from '@angular/core';

/**
 * Centralized date utility service for consistent date transformations
 * across the application.
 */
@Injectable({ providedIn: 'root' })
export class DateUtilService {
  /**
   * Convert a Date or string to input date format (YYYY-MM-DD)
   */
  toInputDate(value: Date | string): string {
    if (typeof value === 'string') {
      return value.slice(0, 10);
    }

    const timezoneOffset = value.getTimezoneOffset() * 60000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 10);
  }

  /**
   * Convert a Date to month input format (YYYY-MM)
   */
  toMonthInput(value: Date): string {
    const timezoneOffset = value.getTimezoneOffset() * 60000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 7);
  }

  /**
   * Convert a date string to display format (localized)
   */
  toDisplayDate(value: string): string {
    const parsed = new Date(`${value.slice(0, 10)}T00:00:00`);
    return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleDateString();
  }

  /**
   * Add days to a date
   */
  addDays(date: Date, days: number): Date {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  }

  /**
   * Check if a date string is in the past
   */
  isPastDate(value: string): boolean {
    const parsed = new Date(`${value.slice(0, 10)}T00:00:00`);
    if (Number.isNaN(parsed.getTime())) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return parsed < today;
  }

  /**
   * Check if a date string is in the future
   */
  isFutureDate(value: string): boolean {
    const parsed = new Date(`${value.slice(0, 10)}T00:00:00`);
    if (Number.isNaN(parsed.getTime())) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    parsed.setHours(0, 0, 0, 0);
    return parsed > today;
  }

  /**
   * Get localized month label from month input (YYYY-MM)
   */
  getMonthLabel(monthValue: string): string {
    const parsed = new Date(`${monthValue}-01T00:00:00`);
    if (Number.isNaN(parsed.getTime())) return monthValue;
    return new Intl.DateTimeFormat(undefined, { month: 'long', year: 'numeric' }).format(parsed);
  }
}
