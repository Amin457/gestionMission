export enum VehicleType  {
    Commercial = 0,
    Passenger = 1,
    Truck = 2
  }
  
  export const VehicleTypeLabel: Record<VehicleType, string> = {
    [VehicleType.Commercial]: 'Commercial',
    [VehicleType.Passenger]: 'Passenger',
    [VehicleType.Truck]: 'Truck'
  };