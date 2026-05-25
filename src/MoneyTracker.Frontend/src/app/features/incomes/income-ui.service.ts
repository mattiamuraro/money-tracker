import { Injectable, signal } from '@angular/core';

/**
 * Manages income feature UI state using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class IncomeUIService {
  readonly isSavingIncome = signal(false);
  readonly successMessage = signal('');
  readonly errorMessage = signal('');

  setSuccessMessage(message: string): void {
    this.successMessage.set(message);
  }

  setErrorMessage(message: string): void {
    this.errorMessage.set(message);
  }

  clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }

  setSavingIncome(saving: boolean): void {
    this.isSavingIncome.set(saving);
  }
}
