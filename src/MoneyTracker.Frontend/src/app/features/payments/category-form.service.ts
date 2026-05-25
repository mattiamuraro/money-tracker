import { Injectable, signal, computed } from '@angular/core';
import { PaymentCategory, PaymentCategoryFormModel } from './payment.models';

/**
 * Manages category form state using Angular signals.
 * Responsible for:
 * - Category form model state (create, edit, reset)
 * - Editing context tracking
 */
@Injectable({ providedIn: 'root' })
export class CategoryFormService {
  readonly editingId = signal<string | null>(null);
  readonly form = signal<PaymentCategoryFormModel>(this.createEmptyForm());
  readonly isEditing = computed(() => this.editingId() !== null);

  startEdit(category: PaymentCategory): void {
    this.editingId.set(category.id);
    this.form.set({
      name: category.name,
      code: category.code,
    });
  }

  updateForm(changes: Partial<PaymentCategoryFormModel>): void {
    this.form.update((current) => ({ ...current, ...changes }));
  }

  reset(): void {
    this.editingId.set(null);
    this.form.set(this.createEmptyForm());
  }

  private createEmptyForm(): PaymentCategoryFormModel {
    return {
      name: '',
      code: '',
    };
  }
}
