import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { IncomeFacadeService } from './income-facade.service';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { DateUtilService } from '../../shared/services/date-util.service';
import { IncomeDataService } from './income-data.service';
import { IncomeFormService } from './income-form.service';
import { IncomeUIService } from './income-ui.service';

describe('IncomeFacadeService', () => {
  let service: IncomeFacadeService;

  const dataStub = {
    incomes: vi.fn(() => []),
    incomeOccurrences: vi.fn(() => []),
    filteredIncomes: vi.fn(() => []),
    filteredOccurrences: vi.fn(() => []),
    incomeTotal: vi.fn(() => 0),
    selectedMonth: vi.fn(() => '2026-01'),
    descriptionFilter: vi.fn(() => ''),
    minAmount: vi.fn(() => null),
    maxAmount: vi.fn(() => null),
    createIncome: vi.fn(),
    updateIncome: vi.fn(),
    deleteIncome: vi.fn(),
    loadIncomes: vi.fn(),
    loadIncomeOccurrences: vi.fn(),
    setSelectedMonth: vi.fn(),
    setDescriptionFilter: vi.fn(),
    setAmountRange: vi.fn(),
    resetFilters: vi.fn(),
  };

  const formStub = {
    form: vi.fn(() => ({ description: '', amount: null, date: '', forecastOccurrenceId: null })),
    isEditing: vi.fn(() => false),
    editingId: vi.fn(() => null),
    startEdit: vi.fn(),
    startFromOccurrence: vi.fn(),
    reset: vi.fn(),
    updateForm: vi.fn(),
    clearForecastOccurrence: vi.fn(),
  };

  const uiStub = {
    isSavingIncome: vi.fn(() => false),
    successMessage: vi.fn(() => ''),
    errorMessage: vi.fn(() => ''),
    setSavingIncome: vi.fn(),
    clearMessages: vi.fn(),
    setSuccessMessage: vi.fn(),
    setErrorMessage: vi.fn(),
  };

  const confirmationDialogStub = {
    show: vi.fn(async () => true),
  };

  const dateUtilStub = {
    isPastDate: vi.fn(() => false),
    toDisplayDate: vi.fn(() => '2026-01-01'),
  };

  beforeEach(() => {
    vi.clearAllMocks();

    TestBed.configureTestingModule({
      providers: [
        IncomeFacadeService,
        { provide: ConfirmationDialogService, useValue: confirmationDialogStub },
        { provide: DateUtilService, useValue: dateUtilStub },
        { provide: IncomeDataService, useValue: dataStub },
        { provide: IncomeFormService, useValue: formStub },
        { provide: IncomeUIService, useValue: uiStub },
      ],
    });

    service = TestBed.inject(IncomeFacadeService);
  });

  it('sets error when submitting invalid income form', async () => {
    await service.submitIncome();

    expect(uiStub.setErrorMessage).toHaveBeenCalled();
    expect(dataStub.createIncome).not.toHaveBeenCalled();
  });

  it('submits valid income and calls createIncome with normalized model', async () => {
    (formStub.form as any).mockReturnValue({
      description: '  Salary  ',
      amount: 2000,
      date: '2026-01-01',
      forecastOccurrenceId: '',
    });

    await service.submitIncome();

    expect(dataStub.createIncome).toHaveBeenCalledWith({
      description: 'Salary',
      amount: 2000,
      date: '2026-01-01',
      forecastOccurrenceId: null,
    });
    expect(uiStub.setSuccessMessage).toHaveBeenCalledWith('Income created successfully.');
  });

  it('passes Reopen action when deleting a past linked occurrence', async () => {
    dateUtilStub.isPastDate.mockReturnValue(true);
    confirmationDialogStub.show.mockResolvedValueOnce(true).mockResolvedValueOnce(true);

    await service.deleteIncome({
      id: 'i-1',
      description: 'Bonus',
      amount: 500,
      date: '2026-01-12',
      forecastOccurrenceId: 'occ-2',
      forecastExpectedDate: '2026-01-08',
    });

    expect(dataStub.deleteIncome).toHaveBeenCalledWith('i-1', 'Reopen');
  });

  it('delegates income filter operations to data service', () => {
    service.setDescriptionFilter('salary');
    service.applyIncomeFilters();
    service.resetFilters();

    expect(dataStub.setDescriptionFilter).toHaveBeenCalledWith('salary');
    expect(dataStub.setSelectedMonth).toHaveBeenCalledWith('2026-01');
    expect(dataStub.resetFilters).toHaveBeenCalled();
  });
});
