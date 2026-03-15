export interface StandupEntry {
  id: string;
  yesterday: string;
  today: string;
  blockers: string | null;
  blockerResolved: boolean;
  createdAt: string;
}
