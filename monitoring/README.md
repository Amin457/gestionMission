# 🚀 Mission Management System - Monitoring & Observability

This directory contains the monitoring and observability configuration for the Mission Management System using **Prometheus** and **Grafana**.

## 📊 **Monitoring Stack Components**

### **1. Prometheus**
- **Purpose**: Metrics collection and storage
- **Port**: 9090
- **URL**: http://localhost:9090
- **Features**:
  - Collects metrics from all services
  - Stores time-series data
  - Provides query language (PromQL)
  - Alerting capabilities

### **2. Grafana**
- **Purpose**: Data visualization and dashboards
- **Port**: 3001
- **URL**: http://localhost:3001
- **Credentials**: admin/admin123
- **Features**:
  - Pre-configured dashboards
  - Real-time monitoring
  - Customizable panels
  - Alert notifications

### **3. Node Exporter**
- **Purpose**: Host system metrics
- **Port**: 9100
- **Metrics**: CPU, memory, disk, network usage

### **4. cAdvisor**
- **Purpose**: Container metrics
- **Port**: 8080
- **Metrics**: Container CPU, memory, network, filesystem

## 🚀 **Quick Start**

### **1. Start Monitoring Stack**
```bash
# Start all services including monitoring
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f prometheus
docker-compose logs -f grafana
```

### **2. Access Monitoring Tools**
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3001 (admin/admin123)
- **cAdvisor**: http://localhost:8080
- **Node Exporter**: http://localhost:9100/metrics

## 📈 **Available Dashboards**

### **Mission Management Dashboard**
- System overview and health status
- Container resource usage (CPU, Memory)
- HTTP request rates and performance
- Database connection status

### **Custom Dashboards**
You can create additional dashboards in Grafana for:
- Mission statistics
- User activity metrics
- Vehicle utilization
- Cost analysis

## 🔧 **Configuration Files**

### **Prometheus Configuration**
- `prometheus.yml`: Main configuration with scrape targets
- Scrapes metrics every 15 seconds
- Monitors all services: backend, frontend, database

### **Grafana Configuration**
- `provisioning/datasources/prometheus.yml`: Prometheus connection
- `provisioning/dashboards/dashboard.yml`: Auto-loading dashboards
- `dashboards/mission-dashboard.json`: Sample dashboard

## 📊 **Key Metrics Monitored**

### **Application Metrics**
- HTTP request rates and response times
- Error rates and status codes
- Database connection status
- Service health endpoints

### **Infrastructure Metrics**
- Container CPU and memory usage
- Host system resources
- Network traffic and disk I/O
- Docker container status

### **Business Metrics**
- Mission creation rates
- User activity patterns
- Vehicle utilization
- Cost tracking

## 🚨 **Alerting (Future Enhancement)**

Configure alerts in Prometheus for:
- High CPU/Memory usage
- Service downtime
- High error rates
- Database connection issues

## 📝 **Custom Metrics**

### **Adding Custom Metrics to Backend**
```csharp
// Example: Add mission count metric
private readonly Counter _missionCounter = Metrics.CreateCounter("missions_total", "Total missions created");

public async Task<MissionDto> CreateMissionAsync(CreateMissionDto dto)
{
    // ... existing code ...
    
    // Increment metric
    _missionCounter.Inc();
    
    return result;
}
```

### **Adding Custom Metrics to Frontend**
```typescript
// Example: Track user interactions
import { Metrics } from './metrics';

export class MissionService {
  createMission(data: any) {
    // ... existing code ...
    
    // Track metric
    Metrics.increment('user_mission_created');
  }
}
```

## 🔍 **Troubleshooting**

### **Common Issues**

1. **Prometheus not accessible**
   ```bash
   docker-compose logs prometheus
   docker-compose exec prometheus wget -qO- http://localhost:9090/-/healthy
   ```

2. **Grafana not accessible**
   ```bash
   docker-compose logs grafana
   docker-compose exec grafana curl -f http://localhost:3000/api/health
   ```

3. **Metrics not showing**
   - Check if services are exposing `/metrics` endpoint
   - Verify Prometheus targets are healthy
   - Check firewall and port configurations

### **Useful Commands**
```bash
# Check all container statuses
docker-compose ps

# View specific service logs
docker-compose logs -f [service-name]

# Access container shell
docker-compose exec [service-name] sh

# Restart monitoring services
docker-compose restart prometheus grafana
```

## 📚 **Additional Resources**

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Docker Monitoring Best Practices](https://docs.docker.com/config/daemon/monitoring/)
- [.NET Core Metrics](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/metrics)

## 🤝 **Contributing**

To add new metrics or dashboards:
1. Update `prometheus.yml` for new targets
2. Create new dashboard JSON files
3. Update this README with new features
4. Test monitoring stack locally before committing

---

**Happy Monitoring! 🎯📊**
