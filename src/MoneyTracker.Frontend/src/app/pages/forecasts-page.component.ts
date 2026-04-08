import { Component } from '@angular/core';
import { App } from '../app';

@Component({
  selector: 'app-forecasts-page',
  templateUrl: './forecasts-page.component.html',
  standalone: false,
})
export class ForecastsPageComponent {
  constructor(public readonly app: App) {}
}
