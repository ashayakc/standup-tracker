import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { StandupEntry } from '../../models/standup-entry';

@Component({
  selector: 'app-standup-list',
  standalone: true,
  imports: [DatePipe],
  template: `
    @for (entry of entries(); track entry.id) {
      <div class="bg-white rounded-lg shadow p-6 mb-4">
        <div class="text-sm text-gray-500 mb-3">
          {{ entry.createdAt | date:'medium' }}
        </div>

        <div class="mb-2">
          <span class="font-medium">Yesterday:</span>
          <p class="text-gray-700">{{ entry.yesterday }}</p>
        </div>

        <div class="mb-2">
          <span class="font-medium">Today:</span>
          <p class="text-gray-700">{{ entry.today }}</p>
        </div>

        @if (entry.blockers) {
          <div
            class="mt-3 p-3 rounded border"
            [class]="entry.blockerResolved
              ? 'bg-green-100 border-green-400 text-green-800'
              : 'bg-amber-100 border-amber-400 text-amber-800'"
          >
            <div class="flex items-center justify-between">
              <div>
                <span class="font-medium">Blocker:</span>
                <span class="ml-1">{{ entry.blockers }}</span>
              </div>
              @if (entry.blockerResolved) {
                <span class="text-sm font-medium">Resolved</span>
              } @else {
                <button
                  (click)="resolve.emit(entry.id)"
                  class="bg-amber-600 text-white text-sm px-3 py-1 rounded hover:bg-amber-700"
                >
                  Resolve
                </button>
              }
            </div>
          </div>
        }
      </div>
    } @empty {
      <p class="text-gray-500 text-center py-8">No standup entries yet. Submit your first one above!</p>
    }
  `,
})
export class StandupListComponent {
  entries = input<StandupEntry[]>([]);
  resolve = output<string>();
}
