# Test Monitoring Setup Script
# This script tests if all monitoring services are running and accessible

Write-Host "🔍 Testing Mission Management System Monitoring..." -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Function to test HTTP endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$ExpectedStatus = "200"
    )
    
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec 10 -ErrorAction Stop
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Host "✅ $Name is accessible at $Url" -ForegroundColor Green
            return $true
        } else {
            Write-Host "⚠️  $Name returned status $($response.StatusCode)" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "❌ $Name is not accessible at $Url" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to test Docker container
function Test-Container {
    param(
        [string]$Name
    )
    
    try {
        $container = docker ps --filter "name=$Name" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        if ($container -and $container -notmatch "No such object") {
            Write-Host "✅ Container $Name is running" -ForegroundColor Green
            Write-Host "   $container" -ForegroundColor Gray
            return $true
        } else {
            Write-Host "❌ Container $Name is not running" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error checking container $Name" -ForegroundColor Red
        return $false
    }
}

Write-Host "`n🐳 Checking Docker containers..." -ForegroundColor Cyan
Test-Container "mission-sqlserver"
Test-Container "mission-backend"
Test-Container "mission-frontend"
Test-Container "mission-prometheus"
Test-Container "mission-grafana"
Test-Container "mission-node-exporter"
Test-Container "mission-cadvisor"

Write-Host "`n🌐 Testing service endpoints..." -ForegroundColor Cyan
Test-Endpoint "Backend API" "http://localhost:5000/health"
Test-Endpoint "Frontend" "http://localhost:3000"
Test-Endpoint "Prometheus" "http://localhost:9090/-/healthy"
Test-Endpoint "Grafana" "http://localhost:3001/api/health"
Test-Endpoint "Node Exporter" "http://localhost:9100/metrics"
Test-Endpoint "cAdvisor" "http://localhost:8081/metrics"

Write-Host "`n📊 Monitoring URLs:" -ForegroundColor Cyan
Write-Host "   Prometheus: http://localhost:9090" -ForegroundColor White
Write-Host "   Grafana:    http://localhost:3001 (admin/admin123)" -ForegroundColor White
Write-Host "   cAdvisor:   http://localhost:8081" -ForegroundColor White
Write-Host "   Node Exporter: http://localhost:9100/metrics" -ForegroundColor White

Write-Host "`n🔍 Checking Docker Compose status..." -ForegroundColor Cyan
try {
    docker-compose ps
} catch {
    Write-Host "❌ Error running docker-compose ps" -ForegroundColor Red
}

Write-Host "`n✨ Monitoring test completed!" -ForegroundColor Green
Write-Host "If all services are running, you can access the monitoring tools at the URLs above." -ForegroundColor White
