# Dashboard Statistics APIs

## Overview
This document describes dedicated APIs for dashboard statistics that provide aggregated data instead of loading all records and calculating on the frontend. This approach is more efficient and scalable.

## Base URL
```
{environment.apiBaseUrl}
```

---

## 1. Dashboard Statistics APIs

### 1.1 Get Dashboard Overview Statistics
**Endpoint:** `GET /dashboard/overview`

**Description:** Retrieves all main dashboard statistics in a single call.

**Request:**
```http
GET /api/dashboard/overview
Authorization: Bearer {JWT_TOKEN}
```

**Response:**
```typescript
interface DashboardOverviewResponse {
  missions: {
    total: number;
    active: number;
    completed: number;
    pending: number;
    rejected: number;
  };
  vehicles: {
    total: number;
    available: number;
    inUse: number;
  };
  drivers: {
    total: number;
    active: number;
    available: number;
    offDuty: number;
  };
  utilization: {
    missionCompletionRate: number;  // Percentage
    vehicleUtilizationRate: number; // Percentage
    driverUtilizationRate: number;  // Percentage
  };
}
```

**Example Response:**
```json
{
  "missions": {
    "total": 150,
    "active": 25,
    "completed": 120,
    "pending": 3,
    "rejected": 2
  },
  "vehicles": {
    "total": 45,
    "available": 32,
    "inUse": 13
  },
  "drivers": {
    "total": 28,
    "active": 18,
    "available": 15,
    "offDuty": 10
  },
  "utilization": {
    "missionCompletionRate": 80.0,
    "vehicleUtilizationRate": 28.9,
    "driverUtilizationRate": 64.3
  }
}
```

### 1.2 Get Mission Statistics
**Endpoint:** `GET /dashboard/missions/stats`

**Description:** Retrieves detailed mission statistics with filtering options.

**Request:**
```http
GET /api/dashboard/missions/stats?dateFrom={ISOString}&dateTo={ISOString}&driverId={number}&type={MissionType}
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters:**
- `dateFrom` (ISO string, optional): Start date for statistics
- `dateTo` (ISO string, optional): End date for statistics
- `driverId` (number, optional): Filter by specific driver
- `type` (MissionType, optional): Filter by mission type

**Response:**
```typescript
interface MissionStatsResponse {
  summary: {
    total: number;
    byStatus: {
      requested: number;
      approved: number;
      planned: number;
      inProgress: number;
      completed: number;
      rejected: number;
    };
    byType: {
      goods: number;
      financial: number;
      administrative: number;
    };
  };
  trends: {
    daily: Array<{
      date: string;
      count: number;
      completed: number;
    }>;
    weekly: Array<{
      week: string;
      count: number;
      completed: number;
    }>;
    monthly: Array<{
      month: string;
      count: number;
      completed: number;
    }>;
  };
  performance: {
    averageCompletionTime: number; // in hours
    completionRate: number; // percentage
    onTimeDeliveryRate: number; // percentage
  };
}
```

### 1.3 Get Vehicle Statistics
**Endpoint:** `GET /dashboard/vehicles/stats`

**Description:** Retrieves vehicle availability and utilization statistics.

**Request:**
```http
GET /api/dashboard/vehicles/stats
Authorization: Bearer {JWT_TOKEN}
```

**Response:**
```typescript
interface VehicleStatsResponse {
  summary: {
    total: number;
    available: number;
    inUse: number;
    maintenance: number;
  };
  byType: {
    car: {
      total: number;
      available: number;
      utilization: number; // percentage
    };
    van: {
      total: number;
      available: number;
      utilization: number;
    };
    truck: {
      total: number;
      available: number;
      utilization: number;
    };
    motorcycle: {
      total: number;
      available: number;
      utilization: number;
    };
  };
  utilization: {
    averageUtilization: number; // percentage
    peakUtilization: number; // percentage
    lowUtilization: number; // percentage
  };
  capacity: {
    totalCapacity: number;
    usedCapacity: number;
    availableCapacity: number;
  };
}
```

### 1.4 Get Driver Statistics
**Endpoint:** `GET /dashboard/drivers/stats`

**Description:** Retrieves driver performance and status statistics.

**Request:**
```http
GET /api/dashboard/drivers/stats?dateFrom={ISOString}&dateTo={ISOString}
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters:**
- `dateFrom` (ISO string, optional): Start date for statistics
- `dateTo` (ISO string, optional): End date for statistics

**Response:**
```typescript
interface DriverStatsResponse {
  summary: {
    total: number;
    active: number;
    available: number;
    offDuty: number;
    onBreak: number;
  };
  performance: {
    topPerformers: Array<{
      driverId: number;
      driverName: string;
      missionsCompleted: number;
      averageRating: number;
      totalDistance: number;
    }>;
    averageMetrics: {
      missionsPerDriver: number;
      averageRating: number;
      averageDistance: number;
    };
  };
  status: {
    byStatus: {
      available: number;
      inTransit: number;
      offDuty: number;
      onBreak: number;
    };
    availabilityTrend: Array<{
      date: string;
      available: number;
      total: number;
    }>;
  };
}
```

### 1.5 Get Chart Data
**Endpoint:** `GET /dashboard/charts/{chartType}`

**Description:** Retrieves specific chart data for dashboard visualizations.

**Request:**
```http
GET /api/dashboard/charts/{chartType}?dateFrom={ISOString}&dateTo={ISOString}
Authorization: Bearer {JWT_TOKEN}
```

**Chart Types:**
- `mission-status` - Mission distribution by status
- `mission-type` - Mission distribution by type
- `vehicle-availability` - Vehicle availability chart
- `driver-status` - Driver status distribution
- `mission-trends` - Mission trends over time
- `utilization-trends` - Utilization trends over time

**Response Examples:**

#### Mission Status Chart
```typescript
interface MissionStatusChartResponse {
  labels: string[];
  datasets: Array<{
    label: string;
    data: number[];
    backgroundColor: string[];
  }>;
}
```

#### Mission Trends Chart
```typescript
interface MissionTrendsChartResponse {
  labels: string[]; // Dates
  datasets: Array<{
    label: string;
    data: number[];
    borderColor: string;
    backgroundColor: string;
  }>;
}
```

### 1.6 Get Recent Activity
**Endpoint:** `GET /dashboard/recent-activity`

**Description:** Retrieves recent dashboard activity for quick overview.

**Request:**
```http
GET /api/dashboard/recent-activity?limit={number}
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters:**
- `limit` (number, optional): Number of recent activities (default: 10)

**Response:**
```typescript
interface RecentActivityResponse {
  activities: Array<{
    id: number;
    type: 'mission_created' | 'mission_completed' | 'vehicle_assigned' | 'driver_status_changed';
    title: string;
    description: string;
    timestamp: string;
    userId?: number;
    userName?: string;
    relatedId?: number; // missionId, vehicleId, etc.
  }>;
  summary: {
    newMissions: number;
    completedMissions: number;
    statusChanges: number;
  };
}
```

---

## 2. Filtered Statistics APIs

### 2.1 Get Filtered Mission Statistics
**Endpoint:** `POST /dashboard/missions/stats/filtered`

**Description:** Retrieves mission statistics based on complex filters.

**Request:**
```http
POST /api/dashboard/missions/stats/filtered
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

{
  "dateRange": {
    "from": "2024-01-01T00:00:00Z",
    "to": "2024-12-31T23:59:59Z"
  },
  "filters": {
    "statuses": [0, 1, 2, 3, 4, 5],
    "types": [0, 1, 2],
    "driverIds": [1, 2, 3],
    "requesters": ["John Doe", "Jane Smith"]
  },
  "groupBy": "status" | "type" | "driver" | "date"
}
```

**Response:**
```typescript
interface FilteredMissionStatsResponse {
  summary: {
    total: number;
    filtered: number;
  };
  groupedData: Array<{
    group: string;
    count: number;
    percentage: number;
  }>;
  trends: Array<{
    period: string;
    count: number;
  }>;
}
```

### 2.2 Get Real-time Statistics
**Endpoint:** `GET /dashboard/realtime`

**Description:** Retrieves real-time dashboard statistics for live updates.

**Request:**
```http
GET /api/dashboard/realtime
Authorization: Bearer {JWT_TOKEN}
```

**Response:**
```typescript
interface RealTimeStatsResponse {
  timestamp: string;
  missions: {
    active: number;
    completedToday: number;
    pending: number;
  };
  vehicles: {
    available: number;
    inUse: number;
  };
  drivers: {
    available: number;
    inTransit: number;
  };
  alerts: Array<{
    type: 'warning' | 'error' | 'info';
    message: string;
    timestamp: string;
  }>;
}
```

---

## 3. DTOs and Interfaces

### 3.1 Base Statistics Interface
```typescript
interface BaseStats {
  total: number;
  percentage: number;
  change: number; // Change from previous period
  trend: 'up' | 'down' | 'stable';
}
```

### 3.2 Date Range Interface
```typescript
interface DateRange {
  from: string; // ISO string
  to: string;   // ISO string
}
```

### 3.3 Filter Interface
```typescript
interface DashboardFilter {
  dateRange?: DateRange;
  statuses?: MissionStatus[];
  types?: MissionType[];
  driverIds?: number[];
  requesters?: string[];
  vehicleTypes?: VehicleType[];
}
```

### 3.4 Chart Data Interface
```typescript
interface ChartData {
  labels: string[];
  datasets: Array<{
    label: string;
    data: number[];
    backgroundColor?: string | string[];
    borderColor?: string;
    borderWidth?: number;
  }>;
}
```

---

## 4. Error Responses

### 4.1 Error Response Format
```typescript
interface ErrorResponse {
  message: string;
  statusCode: number;
  timestamp: string;
  path: string;
  details?: any;
}
```

### 4.2 Common Error Codes
- `400 Bad Request`: Invalid filter parameters
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Chart type not found
- `500 Internal Server Error`: Server error

---

## 5. Usage Examples

### 5.1 Load Dashboard Overview
```typescript
// Load all dashboard statistics
this.http.get<DashboardOverviewResponse>('/api/dashboard/overview')
  .subscribe({
    next: (stats) => {
      this.dashboardStats = stats;
      this.updateStatisticsCards();
    },
    error: (error) => {
      console.error('Error loading dashboard stats:', error);
    }
  });
```

### 5.2 Load Mission Statistics with Filters
```typescript
// Load mission statistics for last 30 days
const dateFrom = new Date();
dateFrom.setDate(dateFrom.getDate() - 30);

this.http.get<MissionStatsResponse>(`/api/dashboard/missions/stats?dateFrom=${dateFrom.toISOString()}`)
  .subscribe({
    next: (stats) => {
      this.missionStats = stats;
      this.updateMissionCharts();
    }
  });
```

### 5.3 Load Chart Data
```typescript
// Load mission status chart
this.http.get<MissionStatusChartResponse>('/api/dashboard/charts/mission-status')
  .subscribe({
    next: (chartData) => {
      this.missionStatusChart = chartData;
      this.updateChart();
    }
  });
```

### 5.4 Real-time Updates
```typescript
// Poll for real-time updates every 30 seconds
setInterval(() => {
  this.http.get<RealTimeStatsResponse>('/api/dashboard/realtime')
    .subscribe({
      next: (realtime) => {
        this.updateRealTimeStats(realtime);
      }
    });
}, 30000);
```

---

## 6. Performance Benefits

### 6.1 Efficiency Improvements
- **Reduced Data Transfer**: Only aggregated statistics instead of full datasets
- **Faster Loading**: Pre-calculated statistics on the server
- **Better Scalability**: Handles large datasets efficiently
- **Reduced Memory Usage**: No need to load all records in frontend

### 6.2 Caching Strategy
- **Statistics Cache**: Cache dashboard statistics for 5 minutes
- **Chart Data Cache**: Cache chart data for 10 minutes
- **Real-time Cache**: Cache real-time data for 30 seconds

### 6.3 Database Optimization
- **Indexed Queries**: Use database indexes for statistics queries
- **Materialized Views**: Pre-calculate common statistics
- **Query Optimization**: Optimize aggregation queries

---

## 7. Implementation Notes

### 7.1 Backend Implementation
```csharp
// Example C# controller structure
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewResponse>> GetOverview()
    {
        // Implement statistics calculation
    }
    
    [HttpGet("missions/stats")]
    public async Task<ActionResult<MissionStatsResponse>> GetMissionStats(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? driverId,
        [FromQuery] MissionType? type)
    {
        // Implement filtered statistics
    }
}
```

### 7.2 Frontend Implementation
```typescript
// Dashboard service
@Injectable({
  providedIn: 'root'
})
export class DashboardStatsService {
  constructor(private http: HttpClient) {}

  getOverview(): Observable<DashboardOverviewResponse> {
    return this.http.get<DashboardOverviewResponse>('/api/dashboard/overview');
  }

  getMissionStats(filters?: any): Observable<MissionStatsResponse> {
    return this.http.get<MissionStatsResponse>('/api/dashboard/missions/stats', { params: filters });
  }

  getChartData(chartType: string): Observable<ChartData> {
    return this.http.get<ChartData>(`/api/dashboard/charts/${chartType}`);
  }
}
```

This approach provides dedicated, efficient APIs for dashboard statistics instead of loading all data and calculating on the frontend. 