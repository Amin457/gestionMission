# Check Required Tools for Jenkins Pipeline
Write-Host "Checking required tools for ProjetPfe Jenkins Pipeline..." -ForegroundColor Green

# Function to check if command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Function to get version
function Get-Version($cmdname) {
    try {
        $version = & $cmdname --version 2>$null
        return $version
    } catch {
        return "Not found"
    }
}

Write-Host "`nTool Check Results:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

# Check .NET
if (Test-Command "dotnet") {
    $dotnetVersion = Get-Version "dotnet"
    Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK: Not found" -ForegroundColor Red
    Write-Host "  Install from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
}

# Check Node.js
if (Test-Command "node") {
    $nodeVersion = Get-Version "node"
    Write-Host "✓ Node.js: $nodeVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js: Not found" -ForegroundColor Red
    Write-Host "  Install from: https://nodejs.org/en/download/" -ForegroundColor Yellow
}

# Check npm
if (Test-Command "npm") {
    $npmVersion = Get-Version "npm"
    Write-Host "✓ npm: $npmVersion" -ForegroundColor Green
} else {
    Write-Host "✗ npm: Not found" -ForegroundColor Red
}

# Check Git
if (Test-Command "git") {
    $gitVersion = Get-Version "git"
    Write-Host "✓ Git: $gitVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Git: Not found" -ForegroundColor Red
    Write-Host "  Install from: https://git-scm.com/download/win" -ForegroundColor Yellow
}

# Check Docker
if (Test-Command "docker") {
    $dockerVersion = Get-Version "docker"
    Write-Host "✓ Docker: $dockerVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Docker: Not found" -ForegroundColor Red
    Write-Host "  Install Docker Desktop from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
}

# Check Docker Compose
if (Test-Command "docker-compose") {
    $composeVersion = Get-Version "docker-compose"
    Write-Host "✓ Docker Compose: $composeVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Docker Compose: Not found" -ForegroundColor Red
}

# Check Jenkins
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080" -UseBasicParsing -TimeoutSec 5
    Write-Host "✓ Jenkins: Running on http://localhost:8080" -ForegroundColor Green
} catch {
    Write-Host "✗ Jenkins: Not running or not accessible" -ForegroundColor Red
    Write-Host "  Start Jenkins service or check if it's running on port 8080" -ForegroundColor Yellow
}

Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Install any missing tools listed above" -ForegroundColor White
Write-Host "2. Start Docker Desktop if not running" -ForegroundColor White
Write-Host "3. Access Jenkins at http://localhost:8080" -ForegroundColor White
Write-Host "4. Follow the plugin setup guide in jenkins-plugins-setup.md" -ForegroundColor White
Write-Host "5. Create the pipeline job using the Jenkinsfile" -ForegroundColor White

Write-Host "`nCheck completed!" -ForegroundColor Green 