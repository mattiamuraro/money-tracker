import { Component } from '@angular/core';
import { App } from '../app';

@Component({
  selector: 'app-payments-page',
  templateUrl: './payments-page.component.html',
  standalone: false,
})
export class PaymentsPageComponent {
  constructor(public readonly app: App) {}
}
