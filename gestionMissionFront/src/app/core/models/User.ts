import { DriverStatus } from "../enums/DriverStatus";

export interface User {
    password: any;
    userId: number;
    firstName: string;
    lastName: string;
    phone: string;
    email: string;
    passwordHash: string;
    role: string;
    currentDriverStatus : DriverStatus;
  }
  