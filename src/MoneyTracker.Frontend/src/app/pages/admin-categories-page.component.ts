import { Component } from '@angular/core';
import { App } from '../app';

@Component({
  selector: 'app-admin-categories-page',
  templateUrl: './admin-categories-page.component.html',
  standalone: false,
})
export class AdminCategoriesPageComponent {
  constructor(public readonly app: App) {}
}
