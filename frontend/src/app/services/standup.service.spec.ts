import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { StandupService } from './standup.service';
import { WeeklySummary } from '../models/weekly-summary';

describe('StandupService', () => {
  let service: StandupService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(StandupService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getWeeklySummary should call GET /api/standups/weekly-summary', () => {
    const mockSummaries: WeeklySummary[] = [
      {
        weekStart: '2026-03-09',
        weekEnd: '2026-03-15',
        standupCount: 5,
        blockersRaised: 2,
        blockersResolved: 1,
        resolutionRate: 50,
        entries: [],
      },
    ];

    service.getWeeklySummary().subscribe((data) => {
      expect(data).toEqual(mockSummaries);
    });

    const req = httpMock.expectOne('/api/standups/weekly-summary');
    expect(req.request.method).toBe('GET');
    req.flush(mockSummaries);
  });
});
