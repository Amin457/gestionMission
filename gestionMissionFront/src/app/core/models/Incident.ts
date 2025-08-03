export enum IncidentType {
  Delay = 0,
  Breakdown = 1,
  LogisticsIssue = 2
}

export enum IncidentStatus {
  Reported = 0,
  InProgress = 1,
  Resolved = 2
}

export interface Incident {
  incidentId: number;
  missionId: number;
  type: IncidentType;
  description: string;
  reportDate: string;
  status: IncidentStatus;
  incidentDocsUrls?: string[] | null;
} 