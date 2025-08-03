import { MissionStatus, MissionType } from "../enums/mission.enums";
import { TaskGet } from "./Task";

export interface Mission {
  missionId: number;
  type: MissionType;
  requesterId: number;
  driverId: number;
  desiredDate: Date;
  systemDate?: Date;
  service: string;
  // quantity: number;
  receiver: string;
  status: MissionStatus;
}

export interface MissionGet extends Mission {
  requester: string;
  driver: string;
  tasks: TaskGet[];
}