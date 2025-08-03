export interface Route {
  routeId: number;
  circuitId: number;
  departureSiteId: number;
  arrivalSiteId: number;
  distanceKm: number;
  departureSiteName: string;
  arrivalSiteName: string;
  ordre: number;
}

export interface Circuit {
  circuitId: number;
  missionId: number;
  departureDate: string;
  departureSiteId: number;
  arrivalSiteId: number;
  departureSiteName: string;
  arrivalSiteName: string;
  routes: Route[];
} 