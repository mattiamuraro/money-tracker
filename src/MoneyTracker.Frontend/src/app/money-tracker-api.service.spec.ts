import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MoneyTrackerApiService } from './money-tracker-api.service';

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

  it('should get forecast income definitions from separated endpoint', async () => {
    const requestPromise = service.getForecastIncomeDefinitions();

    const req = httpMock.expectOne('/api/v1/forecast-incomes/definitions');
    expect(req.request.method).toBe('GET');

    req.flush([]);

    await expect(requestPromise).resolves.toEqual([]);
  });

  it('should get forecast expense definitions from separated endpoint', async () => {
    const requestPromise = service.getForecastExpenseDefinitions();

    const req = httpMock.expectOne('/api/v1/forecast-expenses/definitions');
    expect(req.request.method).toBe('GET');

    req.flush([]);

    await expect(requestPromise).resolves.toEqual([]);
  });

  it('should create forecast income definition using separated endpoint', async () => {
    const requestPromise = service.createForecastIncomeDefinition({
      forecastRecurrenceRuleTypeId: 'rule-id',
      description: 'Salary',
      amount: 3000,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 1,
    });

    const req = httpMock.expectOne('/api/v1/forecast-incomes/definitions');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      forecastRecurrenceRuleTypeId: 'rule-id',
      description: 'Salary',
      amount: 3000,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: null,
      interval: 1,
    });

    req.flush('id');

    await expect(requestPromise).resolves.toBe('id');
  });

  it('should create forecast expense definition using separated endpoint', async () => {
    const requestPromise = service.createForecastExpenseDefinition({
      forecastRecurrenceRuleTypeId: 'rule-id',
      description: 'Rent',
      amount: 1200,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: '',
      interval: 1,
      paymentCategoryId: 'category-id',
    });

    const req = httpMock.expectOne('/api/v1/forecast-expenses/definitions');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      forecastRecurrenceRuleTypeId: 'rule-id',
      description: 'Rent',
      amount: 1200,
      recurrenceStart: '2026-05-01',
      recurrenceEnd: null,
      interval: 1,
      paymentCategoryId: 'category-id',
    });

    req.flush('id');

    await expect(requestPromise).resolves.toBe('id');
  });

  it('should get forecast income occurrences from separated endpoint', async () => {
    const requestPromise = service.getForecastIncomeOccurrences('2026-05');

    const req = httpMock.expectOne((request) => request.url === '/api/v1/forecast-incomes/occurrences');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('month')).toBe('2026-05');

    req.flush([]);

    await expect(requestPromise).resolves.toEqual([]);
  });

  it('should get forecast expense occurrences from separated endpoint', async () => {
    const requestPromise = service.getForecastExpenseOccurrences('2026-05');

    const req = httpMock.expectOne((request) => request.url === '/api/v1/forecast-expenses/occurrences');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('month')).toBe('2026-05');

    req.flush([]);

    await expect(requestPromise).resolves.toEqual([]);
  });
});
