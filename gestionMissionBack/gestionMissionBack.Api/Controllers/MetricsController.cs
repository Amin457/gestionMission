using Microsoft.AspNetCore.Mvc;
using gestionMissionBack.Api.Services;
using Prometheus;

namespace gestionMissionBack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly Counter _customMetricCounter;

        public MetricsController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
            _customMetricCounter = Metrics.CreateCounter("custom_metric_total", "Custom metric for testing");
        }

        [HttpGet("test")]
        public IActionResult TestMetrics()
        {
            // Increment test counter
            _customMetricCounter.Inc();
            
            // Simulate some business metrics
            _metricsService.IncrementMissionCreated();
            _metricsService.SetActiveMissionsCount(5);
            _metricsService.SetVehicleUtilizationRate(75.5);
            
            return Ok(new { 
                message = "Metrics updated successfully",
                timestamp = DateTime.UtcNow,
                customMetricValue = _customMetricCounter.Value
            });
        }

        [HttpGet("status")]
        public IActionResult GetMetricsStatus()
        {
            return Ok(new
            {
                prometheusEnabled = true,
                metricsEndpoint = "/metrics",
                customMetrics = new[]
                {
                    "mission_created_total",
                    "mission_active_count", 
                    "vehicle_utilization_rate",
                    "custom_metric_total"
                },
                timestamp = DateTime.UtcNow
            });
        }
    }
}
