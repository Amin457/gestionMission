import { VehicleType } from "../enums/VehicleType";

export interface Vehicle {
    vehicleId: number;
    type: VehicleType;
    licensePlate: string;
    availability: boolean;
    maxCapacity: number;
    photoUrls?: string[];
}

export interface VehicleFilter {
    type?: VehicleType;
    availability?: boolean;
    licensePlate?: string;
    minCapacity?: number;
    maxCapacity?: number;
}