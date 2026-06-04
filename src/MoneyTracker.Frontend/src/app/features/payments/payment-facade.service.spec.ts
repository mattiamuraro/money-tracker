import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { PaymentFacadeService } from './payment-facade.service';
import { MoneyTrackerApiService } from '../../money-tracker-api.service';
import { ConfirmationDialogService } from '../../shared/services/confirmation-dialog.service';
import { DateUtilService } from '../../shared/services/date-util.service';
import { PaymentDataService } from './payment-data.service';
import { PaymentFormService } from './payment-form.service';
import { CategoryFormService } from './category-form.service';
import { PaymentUIService } from './payment-ui.service';

describe('PaymentFacadeService', () => {
  let service: PaymentFacadeService;

  const dataStub = {
    categories: vi.fn(() => []),
    payments: vi.fn(() => []),
    paymentOccurrences: vi.fn(() => []),
    filteredPayments: vi.fn(() => []),
    filteredOccurrences: vi.fn(() => []),
    paymentTotal: vi.fn(() => 0),
    combinedTotal: vi.fn(() => 0),
    selectedMonth: vi.fn(() => '2026-01'),
    categoryFilter: vi.fn(() => ''),
    descriptionFilter: vi.fn(() => ''),
    minAmount: vi.fn(() => null),
    maxAmount: vi.fn(() => null),
    createPayment: vi.fn(),
    updatePayment: vi.fn(),
    deletePayment: vi.fn(),
    loadCategories: vi.fn(),
    loadPayments: vi.fn(),
    loadPaymentOccurrences: vi.fn(),
    setSelectedMonth: vi.fn(),
    setCategoryFilter: vi.fn(),
    setDescriptionFilter: vi.fn(),
    setAmountRange: vi.fn(),
    resetFilters: vi.fn(),
  };

  const formStub = {
    form: vi.fn(() => ({
      description: '',
      paymentCategoryId: '',
      amount: null,
      date: '',
      isOneShot: true,
      forecastOccurrenceId: null,
    })),
    isEditing: vi.fn(() => false),
    editingId: vi.fn(() => null),
    startEdit: vi.fn(),
    startFromOccurrence: vi.fn(),
    reset: vi.fn(),
    updateForm: vi.fn(),
    clearForecastOccurrence: vi.fn(),
  };

  const categoryFormStub = {
    form: vi.fn(() => ({ name: '', code: '' })),
    isEditing: vi.fn(() => false),
    editingId: vi.fn(() => null),
    startEdit: vi.fn(),
    reset: vi.fn(),
    updateForm: vi.fn(),
  };

  const uiStub = {
    isSavingPayment: vi.fn(() => false),
    isSavingCategory: vi.fn(() => false),
    successMessage: vi.fn(() => ''),
    errorMessage: vi.fn(() => ''),
    setSavingPayment: vi.fn(),
    setSavingCategory: vi.fn(),
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
        PaymentFacadeService,
        { provide: MoneyTrackerApiService, useValue: { createCategory: vi.fn(), updateCategory: vi.fn(), deleteCategory: vi.fn() } },
        { provide: ConfirmationDialogService, useValue: confirmationDialogStub },
        { provide: DateUtilService, useValue: dateUtilStub },
        { provide: PaymentDataService, useValue: dataStub },
        { provide: PaymentFormService, useValue: formStub },
        { provide: CategoryFormService, useValue: categoryFormStub },
        { provide: PaymentUIService, useValue: uiStub },
      ],
    });

    service = TestBed.inject(PaymentFacadeService);
  });

  it('sets error when submitting invalid payment form', async () => {
    await service.submitPayment();

    expect(uiStub.setErrorMessage).toHaveBeenCalled();
    expect(dataStub.createPayment).not.toHaveBeenCalled();
  });

  it('submits valid payment and calls createPayment with normalized model', async () => {
    (formStub.form as any).mockReturnValue({
      description: '  Grocery  ',
      paymentCategoryId: 'cat-1',
      amount: 45,
      date: '2026-01-10',
      isOneShot: true,
      forecastOccurrenceId: '',
    });

    await service.submitPayment();

    expect(dataStub.createPayment).toHaveBeenCalledWith({
      description: 'Grocery',
      paymentCategoryId: 'cat-1',
      amount: 45,
      date: '2026-01-10',
      isOneShot: true,
      forecastOccurrenceId: null,
    });
    expect(uiStub.setSuccessMessage).toHaveBeenCalledWith('Payment created successfully.');
  });

  it('passes Reopen action when deleting a past linked occurrence', async () => {
    dateUtilStub.isPastDate.mockReturnValue(true);
    confirmationDialogStub.show.mockResolvedValueOnce(true).mockResolvedValueOnce(true);

    await service.deletePayment({
      id: 'p-1',
      description: 'Electricity',
      paymentCategoryId: 'cat-1',
      category: 'Bills',
      amount: 80,
      date: '2026-01-11',
      isOneShot: true,
      forecastOccurrenceId: 'occ-1',
      forecastExpectedDate: '2026-01-09',
    });

    expect(dataStub.deletePayment).toHaveBeenCalledWith('p-1', 'Reopen');
  });

  it('delegates payment filter operations to data service', () => {
    service.setCategoryFilter('cat-1');
    service.applyPaymentFilters();
    service.resetFilters();

    expect(dataStub.setCategoryFilter).toHaveBeenCalledWith('cat-1');
    expect(dataStub.setSelectedMonth).toHaveBeenCalledWith('2026-01');
    expect(dataStub.resetFilters).toHaveBeenCalled();
  });
});
