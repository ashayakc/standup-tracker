import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { WeeklySummaryComponent } from './weekly-summary.component';
import { WeeklySummary } from '../../models/weekly-summary';

const mockSummaries: WeeklySummary[] = [
  {
    weekStart: '2026-03-09',
    weekEnd: '2026-03-15',
    standupCount: 5,
    blockersRaised: 3,
    blockersResolved: 2,
    resolutionRate: 66.67,
    entries: [
      {
        id: '1',
        yesterday: 'Worked on feature A',
        today: 'Continue feature A',
        blockers: 'Waiting on API',
        blockerResolved: true,
        createdAt: '2026-03-10T09:00:00Z',
      },
      {
        id: '2',
        yesterday: 'Code review',
        today: 'Fix bugs',
        blockers: null,
        blockerResolved: false,
        createdAt: '2026-03-11T09:00:00Z',
      },
    ],
  },
];

describe('WeeklySummaryComponent', () => {
  let component: WeeklySummaryComponent;
  let fixture: ComponentFixture<WeeklySummaryComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WeeklySummaryComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(WeeklySummaryComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/standups/weekly-summary').flush([]);
    expect(component).toBeTruthy();
  });

  it('should show empty state when summary array is empty', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/standups/weekly-summary').flush([]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(
      'No weekly summary data available yet.'
    );
  });

  it('should render week cards given mock data', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/standups/weekly-summary').flush(mockSummaries);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const cards = compiled.querySelectorAll('.bg-white.rounded-lg');
    expect(cards.length).toBe(1);
  });

  it('should display correct stats', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/standups/weekly-summary').flush(mockSummaries);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const statValues = compiled.querySelectorAll('.text-2xl');

    expect(statValues[0].textContent?.trim()).toBe('5');
    expect(statValues[1].textContent?.trim()).toBe('3');
    expect(statValues[2].textContent?.trim()).toBe('2');
    expect(statValues[3].textContent?.trim()).toContain('67%');
  });

  it('should expand entries on click', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/standups/weekly-summary').flush(mockSummaries);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    // Entries should not be visible initially
    expect(compiled.querySelector('.border-t')).toBeNull();

    // Click the "Show Entries" button
    const toggleButton = compiled.querySelector(
      'button.text-blue-600'
    ) as HTMLButtonElement;
    expect(toggleButton.textContent?.trim()).toBe('Show Entries');
    toggleButton.click();
    fixture.detectChanges();

    // Entries should now be visible
    expect(compiled.querySelector('.border-t')).toBeTruthy();
    expect(compiled.textContent).toContain('Worked on feature A');
    expect(compiled.textContent).toContain('Code review');

    // Button text should change
    expect(toggleButton.textContent?.trim()).toBe('Hide Entries');
  });
});
