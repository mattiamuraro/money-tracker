import { Injectable, signal } from '@angular/core';

/**
 * Manages payment feature UI state using Angular signals.
 * Responsible for:
 * - Loading states for async operations
 * - Success/error messages
 */
@Injectable({ providedIn: 'root' })
export class PaymentUIService {
  readonly isSavingPayment = signal(false);
  readonly isSavingCategory = signal(false);
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

  setSavingPayment(saving: boolean): void {
    this.isSavingPayment.set(saving);
  }

  setSavingCategory(saving: boolean): void {
    this.isSavingCategory.set(saving);
  }
}
