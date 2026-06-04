import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentFacadeService } from '../payments/payment-facade.service';

@Component({
  selector: 'app-admin-categories-page',
  templateUrl: './admin-categories-page.component.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminCategoriesPageComponent {
  constructor(public readonly paymentFacade: PaymentFacadeService) {}

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }
}
