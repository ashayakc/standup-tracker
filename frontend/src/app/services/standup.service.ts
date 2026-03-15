import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StandupEntry } from '../models/standup-entry';
import { CreateStandupRequest } from '../models/create-standup-request';
import { WeeklySummary } from '../models/weekly-summary';

@Injectable({ providedIn: 'root' })
export class StandupService {
  private http = inject(HttpClient);
  private apiUrl = '/api/standups';

  getAll(): Observable<StandupEntry[]> {
    return this.http.get<StandupEntry[]>(this.apiUrl);
  }

  create(request: CreateStandupRequest): Observable<StandupEntry> {
    return this.http.post<StandupEntry>(this.apiUrl, request);
  }

  resolveBlocker(id: string): Observable<StandupEntry> {
    return this.http.patch<StandupEntry>(`${this.apiUrl}/${id}/resolve`, {});
  }

  getWeeklySummary(): Observable<WeeklySummary[]> {
    return this.http.get<WeeklySummary[]>(`${this.apiUrl}/weekly-summary`);
  }

  exportCsv(): void {
    window.open(`${this.apiUrl}/export`, '_blank');
  }
}
