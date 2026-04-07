import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MoneyTrackerApiService } from './money-tracker-api.service';
import { ForecastFormModel, PaymentFormModel } from './models';

describe('MoneyTrackerApiService', () => {
  let service: MoneyTrackerApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MoneyTrackerApiService],
    });

    service = TestBed.inject(MoneyTrackerApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should build payment query params excluding empty values', async () => {
    const requestPromise = service.getPayments({
      pageNumber: 2,
      pageSize: 20,
      sortBy: 'Date',
      sortOrder: 'desc',
      startDate: '2026-04-01',
      endDate: '2026-04-30',
      categoryId: '',
    });

    const req = httpMock.expectOne((request) => request.url === '/api/v1/payments');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('20');
    expect(req.request.params.get('sortBy')).toBe('Date');
    expect(req.request.params.get('sortOrder')).toBe('desc');
    expect(req.request.params.get('startDate')).toBe('2026-04-01');
    expect(req.request.params.get('endDate')).toBe('2026-04-30');
    expect(req.request.params.has('categoryId')).toBe(false);

    req.flush({
      items: [],
      pageNumber: 2,
      pageSize: 20,
      totalItems: 0,
      totalPages: 0,
      hasPreviousPage: true,
      hasNextPage: false,
    });

    await requestPromise;
  });

  it('should send create payment payload with createdBy', async () => {
    const model: PaymentFormModel = {
      description: 'Groceries',
      paymentCategoryId: 'category-id',
      amount: 44.5,
      date: '2026-04-05',
      isOneShot: true,
    };

    const requestPromise = service.createPayment(model);

    const req = httpMock.expectOne('/api/v1/payments');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      description: 'Groceries',
      paymentCategoryId: 'category-id',
      amount: 44.5,
      date: '2026-04-05',
      isOneShot: true,
      createdBy: 'MoneyTracker.Frontend',
    });

    req.flush('new-payment-id');

    await expect(requestPromise).resolves.toBe('new-payment-id');
  });

  it('should send update payment payload with modifiedBy', async () => {
    const model: PaymentFormModel = {
      description: 'Updated',
      paymentCategoryId: 'category-id',
      amount: 10,
      date: '2026-04-06',
      isOneShot: false,
    };

    const requestPromise = service.updatePayment('payment-id', model);

    const req = httpMock.expectOne('/api/v1/payments/payment-id');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({
      description: 'Updated',
      paymentCategoryId: 'category-id',
      amount: 10,
      date: '2026-04-06',
      isOneShot: false,
      modifiedBy: 'MoneyTracker.Frontend',
    });

    req.flush(null);

    await expect(requestPromise).resolves.toBeNull();
  });

  it('should call forecast rows endpoint with required date params', async () => {
    const requestPromise = service.getForecastRows('2026-04-01', '2026-05-01');

    const req = httpMock.expectOne((request) => request.url === '/api/v1/forecasts');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('startDate')).toBe('2026-04-01');
    expect(req.request.params.get('endDate')).toBe('2026-05-01');

    req.flush([]);

    await expect(requestPromise).resolves.toEqual([]);
  });

  it('should send create forecast definition payload and map empty recurrenceEnd to null', async () => {
    const model: ForecastFormModel = {
      description: 'Salary',
      amount: 2500,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '',
      dayInterval: 30,
      isIncome: true,
    };

    const requestPromise = service.createForecastDefinition(model);

    const req = httpMock.expectOne('/api/v1/forecasts/definitions');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      description: 'Salary',
      amount: 2500,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: null,
      dayInterval: 30,
      isIncome: true,
    });

    req.flush('forecast-id');

    await expect(requestPromise).resolves.toBe('forecast-id');
  });

  it('should send update forecast definition payload and preserve recurrenceEnd value', async () => {
    const model: ForecastFormModel = {
      description: 'Rent',
      amount: 900,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '2026-12-31',
      dayInterval: 30,
      isIncome: false,
    };

    const requestPromise = service.updateForecastDefinition('forecast-id', model);

    const req = httpMock.expectOne('/api/v1/forecasts/definitions/forecast-id');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({
      description: 'Rent',
      amount: 900,
      recurrenceStart: '2026-04-01',
      recurrenceEnd: '2026-12-31',
      dayInterval: 30,
      isIncome: false,
    });

    req.flush(null);

    await expect(requestPromise).resolves.toBeNull();
  });

  it('should call delete forecast definition endpoint', async () => {
    const requestPromise = service.deleteForecastDefinition('forecast-id');

    const req = httpMock.expectOne('/api/v1/forecasts/definitions/forecast-id');
    expect(req.request.method).toBe('DELETE');

    req.flush(null);

    await expect(requestPromise).resolves.toBeNull();
  });
});
