import { Injectable, computed, signal } from '@angular/core';
import { DashboardSummary, MoneyTrackerApiService } from '../../money-tracker-api.service';

@Injectable({ providedIn: 'root' })
export class HomeDashboardSummaryService {
  readonly summary = signal<DashboardSummary | null>(null);
  readonly isSummaryLoading = signal(false);
  readonly summaryError = signal<string>('');
  readonly hasSummary = computed(() => this.summary() !== null);

  constructor(private readonly apiService: MoneyTrackerApiService) {}

  async loadSummary(month: string): Promise<void> {
    this.isSummaryLoading.set(true);
    this.summaryError.set('');

    try {
      const result = await this.apiService.getDashboardSummary(month);
      this.summary.set(result);
    } catch {
      this.summary.set(null);
      this.summaryError.set('Dashboard summary is unavailable. Showing live values.');
    } finally {
      this.isSummaryLoading.set(false);
    }
  }
}
