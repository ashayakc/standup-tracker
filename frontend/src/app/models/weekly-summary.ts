import { StandupEntry } from './standup-entry';

export interface WeeklySummary {
  weekStart: string;
  weekEnd: string;
  standupCount: number;
  blockersRaised: number;
  blockersResolved: number;
  resolutionRate: number;
  entries: StandupEntry[];
}
