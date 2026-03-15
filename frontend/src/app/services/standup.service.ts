import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StandupEntry } from '../models/standup-entry';
import { CreateStandupRequest } from '../models/create-standup-request';

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
}
