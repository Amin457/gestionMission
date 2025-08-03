export enum VehicleReservationStatus {
  Requested = 0,
  Approved = 1,
  Rejected = 2
}

export interface VehicleReservation {
  reservationId: number;
  requesterId: number;
  requesterName: string;
  vehicleId: number;
  vehicleLicensePlate: string;
  requiresDriver: boolean;
  departure: string;
  destination: string;
  startDate: string;
  endDate: string;
  status: VehicleReservationStatus;
}

export interface CreateVehicleReservationRequest {
  requesterId: number;
  vehicleId: number;
  requiresDriver: boolean;
  departure: string;
  destination: string;
  startDate: string;
  endDate: string;
  status: VehicleReservationStatus;
}

export interface UpdateVehicleReservationRequest {
  reservationId: number;
  requesterId: number;
  vehicleId: number;
  requiresDriver: boolean;
  departure: string;
  destination: string;
  startDate: string;
  endDate: string;
  status: VehicleReservationStatus;
} 