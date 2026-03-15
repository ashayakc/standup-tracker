import { Component, inject, signal } from '@angular/core';
import { StandupFormComponent } from './components/standup-form/standup-form.component';
import { StandupListComponent } from './components/standup-list/standup-list.component';
import { StandupService } from './services/standup.service';
import { StandupEntry } from './models/standup-entry';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [StandupFormComponent, StandupListComponent],
  template: `
    <div class="min-h-screen bg-gray-100">
      <div class="max-w-2xl mx-auto px-4 py-8">
        <h1 class="text-3xl font-bold text-center mb-8">Standup Tracker</h1>
        <app-standup-form (entryCreated)="loadStandups()" />
        <app-standup-list [entries]="standups()" (resolve)="onResolve($event)" />
      </div>
    </div>
  `,
})
export class AppComponent {
  private standupService = inject(StandupService);
  standups = signal<StandupEntry[]>([]);

  constructor() {
    this.loadStandups();
  }

  loadStandups() {
    this.standupService.getAll().subscribe((entries) => {
      this.standups.set(entries);
    });
  }

  onResolve(id: string) {
    this.standupService.resolveBlocker(id).subscribe(() => {
      this.loadStandups();
    });
  }
}
