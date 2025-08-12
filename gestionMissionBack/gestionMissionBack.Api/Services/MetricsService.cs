using Prometheus;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Api.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly Counter _missionCreatedCounter;
        private readonly Counter _missionCompletedCounter;
        private readonly Gauge _activeMissionsGauge;
        private readonly Histogram _missionDurationHistogram;
        private readonly Counter _vehicleReservationCounter;
        private readonly Gauge _vehicleUtilizationGauge;
        private readonly Counter _incidentReportedCounter;
        private readonly Histogram _httpRequestDurationHistogram;

        public MetricsService()
        {
            // Mission Metrics
            _missionCreatedCounter = Metrics.CreateCounter("mission_created_total", "Total number of missions created");
            _missionCompletedCounter = Metrics.CreateCounter("mission_completed_total", "Total number of missions completed");
            _activeMissionsGauge = Metrics.CreateGauge("mission_active_count", "Number of currently active missions");
            _missionDurationHistogram = Metrics.CreateHistogram("mission_duration_seconds", "Mission duration in seconds", 
                new HistogramConfiguration
                {
                    Buckets = new[] { 60.0, 300.0, 900.0, 1800.0, 3600.0, 7200.0, 14400.0, 28800.0 } // 1min, 5min, 15min, 30min, 1h, 2h, 4h, 8h
                });

            // Vehicle Metrics
            _vehicleReservationCounter = Metrics.CreateCounter("vehicle_reservation_total", "Total number of vehicle reservations");
            _vehicleUtilizationGauge = Metrics.CreateGauge("vehicle_utilization_rate", "Vehicle utilization rate (0-100%)");

            // Incident Metrics
            _incidentReportedCounter = Metrics.CreateCounter("incident_reported_total", "Total number of incidents reported");

            // HTTP Request Metrics
            _httpRequestDurationHistogram = Metrics.CreateHistogram("http_request_duration_seconds", "HTTP request duration in seconds",
                new HistogramConfiguration
                {
                    Buckets = new[] { 0.01, 0.05, 0.1, 0.5, 1.0, 2.0, 5.0, 10.0 }
                });
        }

        public void IncrementMissionCreated() => _missionCreatedCounter.Inc();
        public void IncrementMissionCompleted() => _missionCompletedCounter.Inc();
        public void SetActiveMissionsCount(int count) => _activeMissionsGauge.Set(count);
        public void ObserveMissionDuration(double durationSeconds) => _missionDurationHistogram.Observe(durationSeconds);
        public void IncrementVehicleReservation() => _vehicleReservationCounter.Inc();
        public void SetVehicleUtilizationRate(double rate) => _vehicleUtilizationGauge.Set(rate);
        public void IncrementIncidentReported() => _incidentReportedCounter.Inc();
        public void ObserveHttpRequestDuration(double durationSeconds) => _httpRequestDurationHistogram.Observe(durationSeconds);
    }

    public interface IMetricsService
    {
        void IncrementMissionCreated();
        void IncrementMissionCompleted();
        void SetActiveMissionsCount(int count);
        void ObserveMissionDuration(double durationSeconds);
        void IncrementVehicleReservation();
        void SetVehicleUtilizationRate(double rate);
        void IncrementIncidentReported();
        void ObserveHttpRequestDuration(double durationSeconds);
    }
}
