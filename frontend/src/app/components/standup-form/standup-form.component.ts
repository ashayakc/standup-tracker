import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StandupService } from '../../services/standup.service';

@Component({
  selector: 'app-standup-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <form (ngSubmit)="onSubmit()" class="bg-white rounded-lg shadow p-6 mb-8">
      <h2 class="text-xl font-semibold mb-4">New Standup Entry</h2>

      <div class="mb-4">
        <label for="yesterday" class="block font-medium mb-1">Yesterday</label>
        <textarea
          id="yesterday"
          [(ngModel)]="yesterday"
          name="yesterday"
          rows="2"
          required
          class="w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="What did you do yesterday?"
        ></textarea>
      </div>

      <div class="mb-4">
        <label for="today" class="block font-medium mb-1">Today</label>
        <textarea
          id="today"
          [(ngModel)]="today"
          name="today"
          rows="2"
          required
          class="w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="What will you do today?"
        ></textarea>
      </div>

      <div class="mb-4">
        <label for="blockers" class="block font-medium mb-1">Blockers</label>
        <textarea
          id="blockers"
          [(ngModel)]="blockers"
          name="blockers"
          rows="2"
          class="w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Any blockers? (optional)"
        ></textarea>
      </div>

      <button
        type="submit"
        [disabled]="!yesterday.trim() || !today.trim()"
        class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        Submit
      </button>
    </form>
  `,
})
export class StandupFormComponent {
  private standupService = inject(StandupService);

  yesterday = '';
  today = '';
  blockers = '';

  entryCreated = output<void>();

  onSubmit() {
    this.standupService
      .create({
        yesterday: this.yesterday,
        today: this.today,
        blockers: this.blockers.trim() || null,
      })
      .subscribe(() => {
        this.yesterday = '';
        this.today = '';
        this.blockers = '';
        this.entryCreated.emit();
      });
  }
}
