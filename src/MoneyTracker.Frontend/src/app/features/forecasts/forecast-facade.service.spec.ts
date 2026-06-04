import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ForecastFacadeService } from './forecast-facade.service';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { ForecastOccurrenceRow } from './forecast.models';
import { ForecastDataService } from './forecast-data.service';
import { ForecastFormService } from './forecast-form.service';
import { ForecastUIService } from './forecast-ui.service';
import { PaymentDataService } from '../payments/payment-data.service';
import { IncomeDataService } from '../incomes/income-data.service';

describe('ForecastFacadeService', () => {
  let service: ForecastFacadeService;

  const dataStub = {
    recurrenceRuleTypes: vi.fn(() => []),
    incomeDefinitions: vi.fn(() => []),
    expenseDefinitions: vi.fn(() => []),
    incomeRows: vi.fn(() => []),
    expenseRows: vi.fn(() => []),
    incomeTotal: vi.fn(() => 0),
    expenseTotal: vi.fn(() => 0),
    balance: vi.fn(() => 0),
    upcomingIncomes: vi.fn(() => []),
    upcomingExpenses: vi.fn(() => []),
    rangeStart: vi.fn(() => '2026-01-01'),
    rangeEnd: vi.fn(() => '2026-01-31'),
    isOneTimeRecurrenceType: vi.fn(() => true),
    createIncomeDefinition: vi.fn(),
    updateIncomeDefinition: vi.fn(),
    deleteIncomeDefinition: vi.fn(),
    createExpenseDefinition: vi.fn(),
    updateExpenseDefinition: vi.fn(),
    deleteExpenseDefinition: vi.fn(),
    discardIncomeOccurrence: vi.fn(),
    discardExpenseOccurrence: vi.fn(),
    setDateRange: vi.fn(),
    loadRecurrenceRuleTypes: vi.fn(),
    loadDefinitions: vi.fn(),
    loadRows: vi.fn(),
    getRecurrenceTypeLabel: vi.fn(() => 'One Time (O)'),
    getRecurrenceSummary: vi.fn(() => 'One time'),
  };

  const formStub = {
    incomeForm: vi.fn(() => ({
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: '',
      recurrenceEnd: '',
      interval: 1,
    })),
    isEditingIncome: vi.fn(() => false),
    editingIncomeId: vi.fn(() => null),
    expenseForm: vi.fn(() => ({
      forecastRecurrenceRuleTypeId: '',
      description: '',
      amount: null,
      recurrenceStart: '',
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: '',
    })),
    isEditingExpense: vi.fn(() => false),
    editingExpenseId: vi.fn(() => null),
    updateIncomeForm: vi.fn(),
    updateExpenseForm: vi.fn(),
    resetIncomeForm: vi.fn(),
    resetExpenseForm: vi.fn(),
    startIncomeEdit: vi.fn(),
    startExpenseEdit: vi.fn(),
  };

  const uiStub = {
    isSavingIncome: vi.fn(() => false),
    isSavingExpense: vi.fn(() => false),
    successMessage: vi.fn(() => ''),
    errorMessage: vi.fn(() => ''),
    setSavingIncome: vi.fn(),
    setSavingExpense: vi.fn(),
    clearMessages: vi.fn(),
    setSuccessMessage: vi.fn(),
    setErrorMessage: vi.fn(),
  };

  const confirmationDialogStub = {
    show: vi.fn(async () => true),
  };

  const paymentDataStub = {
    loadPaymentOccurrences: vi.fn(),
    loadPayments: vi.fn(),
  };

  const incomeDataStub = {
    loadIncomeOccurrences: vi.fn(),
    loadIncomes: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();

    TestBed.configureTestingModule({
      providers: [
        ForecastFacadeService,
        { provide: ConfirmationDialogService, useValue: confirmationDialogStub },
        { provide: ForecastDataService, useValue: dataStub },
        { provide: ForecastFormService, useValue: formStub },
        { provide: ForecastUIService, useValue: uiStub },
        { provide: PaymentDataService, useValue: paymentDataStub },
        { provide: IncomeDataService, useValue: incomeDataStub },
      ],
    });

    service = TestBed.inject(ForecastFacadeService);
  });

  it('returns recurrence label from data service', () => {
    const label = service.getRecurrenceTypeLabel('rule-1');

    expect(label).toBe('One Time (O)');
    expect(dataStub.getRecurrenceTypeLabel).toHaveBeenCalledWith('rule-1');
  });

  it('sets error when submitting invalid forecast income form', async () => {
    await service.submitIncomeDefinition();

    expect(uiStub.setErrorMessage).toHaveBeenCalled();
    expect(dataStub.createIncomeDefinition).not.toHaveBeenCalled();
  });

  it('submits valid forecast expense definition and calls create', async () => {
    (formStub.expenseForm as any).mockReturnValue({
      forecastRecurrenceRuleTypeId: 'rule-2',
      description: '  Rent  ',
      amount: 900,
      recurrenceStart: '2026-01-01',
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: 'cat-1',
    });
    dataStub.isOneTimeRecurrenceType.mockReturnValue(false);

    await service.submitExpenseDefinition();

    expect(dataStub.createExpenseDefinition).toHaveBeenCalledWith({
      forecastRecurrenceRuleTypeId: 'rule-2',
      description: 'Rent',
      amount: 900,
      recurrenceStart: '2026-01-01',
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: 'cat-1',
    });
    expect(uiStub.setSuccessMessage).toHaveBeenCalledWith('Forecast expense created successfully.');
    expect(paymentDataStub.loadPaymentOccurrences).toHaveBeenCalled();
  });

  it('discards income occurrence and refreshes forecast/income data', async () => {
    confirmationDialogStub.show.mockResolvedValue(true);

    await service.discardForecastOccurrence(
      {
        id: 'occ-9',
        forecastDefinitionId: 'def-1',
        description: 'Projected Salary',
        expectedDate: '2026-01-25',
        amount: 1500,
        isIncome: true,
      } as ForecastOccurrenceRow,
      true
    );

    expect(dataStub.discardIncomeOccurrence).toHaveBeenCalledWith('occ-9');
    expect(incomeDataStub.loadIncomeOccurrences).toHaveBeenCalled();
    expect(incomeDataStub.loadIncomes).toHaveBeenCalled();
    expect(dataStub.loadRows).toHaveBeenCalled();
  });
});
