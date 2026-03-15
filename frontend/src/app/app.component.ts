import { Component, inject, signal } from '@angular/core';
import { StandupFormComponent } from './components/standup-form/standup-form.component';
import { StandupListComponent } from './components/standup-list/standup-list.component';
import { WeeklySummaryComponent } from './components/weekly-summary/weekly-summary.component';
import { StandupService } from './services/standup.service';
import { StandupEntry } from './models/standup-entry';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [StandupFormComponent, StandupListComponent, WeeklySummaryComponent],
  template: `
    <div class="min-h-screen bg-gray-100">
      <div class="max-w-2xl mx-auto px-4 py-8">
        <h1 class="text-3xl font-bold text-center mb-8">Standup Tracker</h1>

        <div class="flex justify-center mb-6">
          <button
            (click)="activeView.set('daily')"
            [class]="'px-4 py-2 font-medium rounded-l-lg border ' + (activeView() === 'daily'
              ? 'bg-blue-600 text-white border-blue-600'
              : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50')"
          >
            Daily
          </button>
          <button
            (click)="activeView.set('weekly')"
            [class]="'px-4 py-2 font-medium rounded-r-lg border border-l-0 ' + (activeView() === 'weekly'
              ? 'bg-blue-600 text-white border-blue-600'
              : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50')"
          >
            Weekly Summary
          </button>
        </div>

        @if (activeView() === 'daily') {
          <div class="flex justify-end mb-4">
            <button
              (click)="onExport()"
              class="border border-gray-300 bg-white text-gray-700 px-4 py-2 rounded hover:bg-gray-50 text-sm font-medium"
            >
              Export CSV
            </button>
          </div>
          <app-standup-form (entryCreated)="loadStandups()" />
          <app-standup-list [entries]="standups()" (resolve)="onResolve($event)" />
        } @else {
          <app-weekly-summary />
        }
      </div>
    </div>
  `,
})
export class AppComponent {
  private standupService = inject(StandupService);
  standups = signal<StandupEntry[]>([]);
  activeView = signal<'daily' | 'weekly'>('daily');

  constructor() {
    this.loadStandups();
  }

  loadStandups() {
    this.standupService.getAll().subscribe((entries) => {
      this.standups.set(entries);
    });
  }

  onExport() {
    this.standupService.exportCsv();
  }

  onResolve(id: string) {
    this.standupService.resolveBlocker(id).subscribe(() => {
      this.loadStandups();
    });
  }
}
