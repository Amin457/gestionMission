export enum DriverStatus {
    OffDuty = 0,
    OnBreak = 1,
    OnLeave = 2,
    InTransit = 3
  }
  
  export const DriverStatusLabel: Record<DriverStatus, string> = {
    [DriverStatus.OffDuty]: 'Off Duty',
    [DriverStatus.OnBreak]: 'On Break',
    [DriverStatus.OnLeave]: 'On Leave',
    [DriverStatus.InTransit]: 'In Transit'
  };