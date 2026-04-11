import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { App } from './app';
import { MoneyTrackerApiService } from './money-tracker-api.service';

describe('App', () => {
  let apiServiceSpy: {
    getCategories: ReturnType<typeof vi.fn>;
    getPayments: ReturnType<typeof vi.fn>;
    getForecastRecurrenceRuleTypes: ReturnType<typeof vi.fn>;
    getForecastDefinitions: ReturnType<typeof vi.fn>;
    getForecastRows: ReturnType<typeof vi.fn>;
    createPayment: ReturnType<typeof vi.fn>;
    updatePayment: ReturnType<typeof vi.fn>;
    deletePayment: ReturnType<typeof vi.fn>;
    createForecastDefinition: ReturnType<typeof vi.fn>;
    updateForecastDefinition: ReturnType<typeof vi.fn>;
    deleteForecastDefinition: ReturnType<typeof vi.fn>;
  };

  const getDefaultPaginatedPayments = () => ({
    items: [],
    pageNumber: 1,
    pageSize: 12,
    totalItems: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });

  beforeEach(async () => {
    apiServiceSpy = {
      getCategories: vi.fn().mockResolvedValue([]),
      getPayments: vi.fn().mockResolvedValue(getDefaultPaginatedPayments()),
      getForecastRecurrenceRuleTypes: vi.fn().mockResolvedValue([
        { id: 'one-time-type-id', name: 'One Time', code: 'O' },
        { id: 'day-type-id', name: 'Day', code: 'D' },
        { id: 'week-type-id', name: 'Week', code: 'W' },
      ]),
      getForecastDefinitions: vi.fn().mockResolvedValue([]),
      getForecastRows: vi.fn().mockResolvedValue([]),
      createPayment: vi.fn().mockResolvedValue('00000000-0000-0000-0000-000000000000'),
      updatePayment: vi.fn().mockResolvedValue(undefined),
      deletePayment: vi.fn().mockResolvedValue(undefined),
      createForecastDefinition: vi.fn().mockResolvedValue('00000000-0000-0000-0000-000000000000',
      updateForecastDefinition: vi.fn().mockResolvedValue(undefined),
      deleteForecastDefinition: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [CommonModule, FormsModule],
      declarations: [App],
      providers: [{ provide: MoneyTrackerApiService, useValue: apiServiceSpy }],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the dashboard title', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Money Tracker');
    expect(compiled.textContent).toContain('Payments');
    expect(compiled.textContent).toContain('Forecasts');
  });

  it('should load recurrence rule types and set default forecast recurrence type to one time', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    await app.reloadDashboard();

    expect(apiServiceSpy.getForecastRecurrenceRuleTypes).toHaveBeenCalled();
    expect(app.forecastRecurrenceRuleTypes.length).toBeGreaterThan(0);
    expect(app.forecastForm.forecastRecurrenceRuleTypeId).toBe('one-time-type-id');
    expect(app.forecastForm.interval).toBe(1);
  });

  it('should reject payment submit when required fields are missing', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.paymentForm.description = '   ';
    app.paymentForm.paymentCategoryId = '';
    app.paymentForm.amount = null;

    await app.submitPayment();

    expect(app.errorMessage).toBe('Complete all required payment fields before saving.');
    expect(apiServiceSpy.createPayment).not.toHaveBeenCalled();
    expect(apiServiceSpy.updatePayment).not.toHaveBeenCalled();
  });

  it('should create payment when not in edit mode', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.paymentForm = {
      description: '  Groceries  ',
      paymentCategoryId: 'category-id',
      amount: 25,
      date: '2026-04-05',
      isOneShot: true,
    };

    await app.submitPayment();

    expect(apiServiceSpy.createPayment).toHaveBeenCalledWith({
      description: 'Groceries',
      paymentCategoryId: 'category-id',
      amount: 25,
      date: '2026-04-05',
      isOneShot: true,
    });
    expect(apiServiceSpy.updatePayment).not.toHaveBeenCalled();
    expect(app.successMessage).toBe('Payment created successfully.');
    expect(app.editingPaymentId).toBeNull();
  });

  it('should update payment when in edit mode', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.editingPaymentId = 'payment-id';
    app.paymentForm = {
      description: '  Updated payment  ',
      paymentCategoryId: 'category-id',
      amount: 99,
      date: '2026-04-06',
      isOneShot: false,
    };

    await app.submitPayment();

    expect(apiServiceSpy.updatePayment).toHaveBeenCalledWith('payment-id', {
      description: 'Updated payment',
      paymentCategoryId: 'category-id',
      amount: 99,
      date: '2026-04-06',
      isOneShot: false,
    });
    expect(apiServiceSpy.createPayment).not.toHaveBeenCalled();
    expect(app.successMessage).toBe('Payment updated successfully.');
    expect(app.editingPaymentId).toBeNull();
  });

  it('should reject forecast submit when required fields are missing', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.forecastForm.description = '   ';
    app.forecastForm.amount = null;
    app.forecastForm.interval = 0;

    await app.submitForecast();

    expect(app.errorMessage).toBe('Complete all required forecast fields before saving.');
    expect(apiServiceSpy.createForecastDefinition).not.toHaveBeenCalled();
    expect(apiServiceSpy.updateForecastDefinition).not.toHaveBeenCalled();
  });

  it('should create forecast when not in edit mode', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.forecastForm = {
      forecastRecurrenceRuleTypeId: '11111111-1111-1111-1111-111111111111',
      description: '  Monthly Rent  ',
      amount: 800,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '',
      interval: 30,
      isIncome: false,
    };

    await app.submitForecast();

    expect(apiServiceSpy.createForecastDefinition).toHaveBeenCalledWith({
      forecastRecurrenceRuleTypeId: '11111111-1111-1111-1111-111111111111',
      description: 'Monthly Rent',
      amount: 800,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '',
      interval: 30,
      isIncome: false,
    });
    expect(apiServiceSpy.updateForecastDefinition).not.toHaveBeenCalled();
    expect(app.successMessage).toBe('Forecast created successfully.');
    expect(app.editingForecastId).toBeNull();
  });

  it('should force interval to one when recurrence type is one time', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.forecastRecurrenceRuleTypes = [{ id: 'one-time-type-id', name: 'One Time', code: 'O' }];
    app.forecastForm = {
      forecastRecurrenceRuleTypeId: 'one-time-type-id',
      description: '  Gift  ',
      amount: 120,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '',
      interval: 12,
      isIncome: true,
    };

    await app.submitForecast();

    expect(apiServiceSpy.createForecastDefinition).toHaveBeenCalledWith({
      forecastRecurrenceRuleTypeId: 'one-time-type-id',
      description: 'Gift',
      amount: 120,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '',
      interval: 1,
      isIncome: true,
    });
  });

  it('should update forecast when in edit mode', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.editingForecastId = 'forecast-id';
    app.forecastForm = {
      forecastRecurrenceRuleTypeId: '22222222-2222-2222-2222-222222222222',
      description: '  Updated Salary  ',
      amount: 3000,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '2026-12-31',
      interval: 30,
      isIncome: true,
    };

    await app.submitForecast();

    expect(apiServiceSpy.updateForecastDefinition).toHaveBeenCalledWith('forecast-id', {
      forecastRecurrenceRuleTypeId: '22222222-2222-2222-2222-222222222222',
      description: 'Updated Salary',
      amount: 3000,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '2026-12-31',
      interval: 30,
      isIncome: true,
    });
    expect(apiServiceSpy.createForecastDefinition).not.toHaveBeenCalled();
    expect(app.successMessage).toBe('Forecast updated successfully.');
    expect(app.editingForecastId).toBeNull();
  });

  it('should not delete forecast when confirmation is canceled', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const definition = {
      id: 'forecast-id',
      forecastRecurrenceRuleTypeId: '33333333-3333-3333-3333-333333333333',
      description: 'Salary',
      amount: 1000,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: null,
      interval: 30,
      isIncome: true,
    };

    await app.deleteForecast(definition);

    expect(apiServiceSpy.deleteForecastDefinition).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('should compute payment and forecast totals', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.payments = [
      {
        id: 'p1',
        description: 'A',
        paymentCategoryId: 'c1',
        category: 'Food',
        amount: 10,
        date: '2026-04-01',
        isOneShot: true,
      },
      {
        id: 'p2',
        description: 'B',
        paymentCategoryId: 'c1',
        category: 'Food',
        amount: 15.5,
        date: '2026-04-02',
        isOneShot: false,
      },
    ];

    app.forecastRows = [
      { id: 'f1', description: 'Salary', amount: 1000, date: '2026-04-10', isIncome: true },
      { id: 'f2', description: 'Rent', amount: 400, date: '2026-04-11', isIncome: false },
      { id: 'f3', description: 'Bills', amount: 100, date: '2026-04-12', isIncome: false },
    ];

    expect(app.paymentTotal).toBe(25.5);
    expect(app.forecastIncomeTotal).toBe(1000);
    expect(app.forecastExpenseTotal).toBe(500);
    expect(app.forecastBalance).toBe(500);
  });
});
