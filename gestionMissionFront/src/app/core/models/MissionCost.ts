export enum MissionCostType {
  Fuel = 0,
  Toll = 1,
  Maintenance = 2
}

export interface MissionCost {
  costId: number;
  missionId: number;
  type: MissionCostType;
  amount: number;
  date: string;
  receiptPhotoUrls?: string[] | null;
} 