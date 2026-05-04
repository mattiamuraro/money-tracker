import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { App } from './app';
import { MoneyTrackerApiService } from './money-tracker-api.service';
import { AuthService } from './auth/auth.service';
import { Router } from '@angular/router';
import { ConfirmationDialogService } from './services/confirmation-dialog.service';
import { NgZone } from '@angular/core';

describe('App', () => {
  const apiServiceSpy = {
    getCategories: vi.fn().mockResolvedValue([]),
    getPayments: vi.fn().mockResolvedValue({ items: [], totalItems: 0, pageNumber: 1, pageSize: 100 }),
    getIncomes: vi.fn().mockResolvedValue({ items: [], totalItems: 0, pageNumber: 1, pageSize: 100 }),
    getForecastRecurrenceRuleTypes: vi.fn().mockResolvedValue([{ id: 'one-time-type-id', name: 'One Time', code: 'O' }]),
    getForecastIncomeDefinitions: vi.fn().mockResolvedValue([]),
    getForecastExpenseDefinitions: vi.fn().mockResolvedValue([]),
    getForecastIncomeRows: vi.fn().mockResolvedValue([]),
    getForecastExpenseRows: vi.fn().mockResolvedValue([]),
    getForecastIncomeOccurrences: vi.fn().mockResolvedValue([]),
    getForecastExpenseOccurrences: vi.fn().mockResolvedValue([]),
    getForecastOccurrences: vi.fn().mockResolvedValue([]),
    createForecastIncomeDefinition: vi.fn().mockResolvedValue('new-id'),
    updateForecastIncomeDefinition: vi.fn().mockResolvedValue(undefined),
    deleteForecastIncomeDefinition: vi.fn().mockResolvedValue(undefined),
    createForecastExpenseDefinition: vi.fn().mockResolvedValue('new-id'),
    updateForecastExpenseDefinition: vi.fn().mockResolvedValue(undefined),
    deleteForecastExpenseDefinition: vi.fn().mockResolvedValue(undefined),
    discardForecastIncomeOccurrence: vi.fn().mockResolvedValue(undefined),
    discardForecastExpenseOccurrence: vi.fn().mockResolvedValue(undefined),
    createPayment: vi.fn().mockResolvedValue('new-id'),
    updatePayment: vi.fn().mockResolvedValue(undefined),
    deletePayment: vi.fn().mockResolvedValue(undefined),
    createIncome: vi.fn().mockResolvedValue('new-id'),
    updateIncome: vi.fn().mockResolvedValue(undefined),
    deleteIncome: vi.fn().mockResolvedValue(undefined),
    createCategory: vi.fn().mockResolvedValue('new-id'),
    updateCategory: vi.fn().mockResolvedValue(undefined),
    deleteCategory: vi.fn().mockResolvedValue(undefined),
  };

  const authServiceSpy = {
    logout: vi.fn(),
  };

  const routerSpy = {
    url: '/',
    events: { pipe: () => ({ subscribe: () => ({}) }) },
  };

  const confirmationDialogServiceSpy = {
    getDialogState: vi.fn().mockReturnValue({ subscribe: () => ({}) }),
    show: vi.fn().mockResolvedValue(true),
    confirm: vi.fn(),
    cancel: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      declarations: [App],
      providers: [
        { provide: MoneyTrackerApiService, useValue: apiServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ConfirmationDialogService, useValue: confirmationDialogServiceSpy },
        { provide: NgZone, useFactory: () => new NgZone({ enableLongStackTrace: false }) },
      ],
    }).compileComponents();
  });

  it('should create the app component', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should create forecast expense using separated expense API', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.forecastRecurrenceRuleTypes = [{ id: 'one-time-type-id', name: 'One Time', code: 'O' }];
    app.forecastExpenseForm = {
      forecastRecurrenceRuleTypeId: 'one-time-type-id',
      description: '  Rent  ',
      amount: 1200,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 30,
      paymentCategoryId: 'category-1',
    };

    await app.submitForecastExpense();

    expect(apiServiceSpy.createForecastExpenseDefinition).toHaveBeenCalledWith({
      forecastRecurrenceRuleTypeId: 'one-time-type-id',
      description: 'Rent',
      amount: 1200,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: 'category-1',
    });
    expect(app.successMessage).toBe('Forecast expense created successfully.');
  });

  it('should create forecast income using separated income API', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.forecastRecurrenceRuleTypes = [{ id: 'month-type-id', name: 'Month', code: 'M' }];
    app.forecastIncomeForm = {
      forecastRecurrenceRuleTypeId: 'month-type-id',
      description: '  Salary  ',
      amount: 3000,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 1,
    };

    await app.submitForecastIncome();

    expect(apiServiceSpy.createForecastIncomeDefinition).toHaveBeenCalledWith({
      forecastRecurrenceRuleTypeId: 'month-type-id',
      description: 'Salary',
      amount: 3000,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 1,
    });
    expect(app.successMessage).toBe('Forecast income created successfully.');
  });
});
