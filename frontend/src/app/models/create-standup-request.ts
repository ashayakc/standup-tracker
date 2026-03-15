export interface CreateStandupRequest {
  yesterday: string;
  today: string;
  blockers: string | null;
}
