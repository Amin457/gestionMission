import { TasksStatus } from "../enums/TasksStatus";

export interface Task {
    taskId: number;
    description: string;
    assignmentDate: Date;
    completionDate: Date;
    status: TasksStatus;
    missionId: number;
    siteId: number;
    isFirstTask: boolean;
  }
export interface TaskGet extends Task  {
    siteName: string;
  }