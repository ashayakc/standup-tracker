import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { StandupService } from '../../services/standup.service';
import { WeeklySummary } from '../../models/weekly-summary';

@Component({
  selector: 'app-weekly-summary',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  template: `
    @if (summaries().length === 0) {
      <p class="text-gray-500 text-center py-8">No weekly summary data available yet.</p>
    } @else {
      @for (summary of summaries(); track summary.weekStart) {
        <div class="bg-white rounded-lg shadow p-6 mb-4">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold">
              {{ summary.weekStart | date:'mediumDate' }} - {{ summary.weekEnd | date:'mediumDate' }}
            </h3>
            <button
              (click)="toggleExpand(summary.weekStart)"
              class="text-blue-600 hover:text-blue-800 text-sm font-medium"
            >
              {{ expandedWeeks().has(summary.weekStart) ? 'Hide Entries' : 'Show Entries' }}
            </button>
          </div>

          <div class="grid grid-cols-2 gap-4 md:grid-cols-4">
            <div class="text-center p-3 bg-blue-50 rounded">
              <div class="text-2xl font-bold text-blue-700">{{ summary.standupCount }}</div>
              <div class="text-sm text-gray-600">Standups</div>
            </div>
            <div class="text-center p-3 bg-amber-50 rounded">
              <div class="text-2xl font-bold text-amber-700">{{ summary.blockersRaised }}</div>
              <div class="text-sm text-gray-600">Blockers Raised</div>
            </div>
            <div class="text-center p-3 bg-green-50 rounded">
              <div class="text-2xl font-bold text-green-700">{{ summary.blockersResolved }}</div>
              <div class="text-sm text-gray-600">Blockers Resolved</div>
            </div>
            <div class="text-center p-3 bg-purple-50 rounded">
              <div class="text-2xl font-bold text-purple-700">{{ summary.resolutionRate | number:'1.0-0' }}%</div>
              <div class="text-sm text-gray-600">Resolution Rate</div>
            </div>
          </div>

          @if (expandedWeeks().has(summary.weekStart)) {
            <div class="mt-4 border-t pt-4">
              <h4 class="font-medium text-gray-700 mb-3">Entries</h4>
              @for (entry of summary.entries; track entry.id) {
                <div class="bg-gray-50 rounded p-4 mb-2">
                  <div class="text-sm text-gray-500 mb-2">{{ entry.createdAt | date:'medium' }}</div>
                  <div class="mb-1"><span class="font-medium">Yesterday:</span> {{ entry.yesterday }}</div>
                  <div class="mb-1"><span class="font-medium">Today:</span> {{ entry.today }}</div>
                  @if (entry.blockers) {
                    <div class="mt-1">
                      <span class="font-medium">Blocker:</span>
                      <span [class]="entry.blockerResolved ? 'text-green-700' : 'text-amber-700'">
                        {{ entry.blockers }}
                        {{ entry.blockerResolved ? '(Resolved)' : '(Open)' }}
                      </span>
                    </div>
                  }
                </div>
              }
            </div>
          }
        </div>
      }
    }
  `,
})
export class WeeklySummaryComponent implements OnInit {
  private standupService = inject(StandupService);
  summaries = signal<WeeklySummary[]>([]);
  expandedWeeks = signal<Set<string>>(new Set());

  ngOnInit() {
    this.standupService.getWeeklySummary().subscribe((data) => {
      this.summaries.set(data);
    });
  }

  toggleExpand(weekStart: string) {
    const current = new Set(this.expandedWeeks());
    if (current.has(weekStart)) {
      current.delete(weekStart);
    } else {
      current.add(weekStart);
    }
    this.expandedWeeks.set(current);
  }
}
