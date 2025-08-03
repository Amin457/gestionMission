# Jenkins Setup Script for ProjetPfe Pipeline
# Run this script as Administrator

Write-Host "Setting up Jenkins for ProjetPfe Pipeline..." -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Please run this script as Administrator" -ForegroundColor Red
    exit 1
}

# Function to check if command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Function to install Chocolatey if not present
function Install-Chocolatey {
    if (-not (Test-Command "choco")) {
        Write-Host "Installing Chocolatey..." -ForegroundColor Yellow
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    } else {
        Write-Host "Chocolatey is already installed" -ForegroundColor Green
    }
}

# Function to install .NET 8.0 SDK
function Install-DotNetSDK {
    if (-not (Test-Command "dotnet")) {
        Write-Host "Installing .NET 8.0 SDK..." -ForegroundColor Yellow
        choco install dotnet-8.0-sdk -y
    } else {
        $version = dotnet --version
        Write-Host ".NET SDK is already installed: $version" -ForegroundColor Green
    }
}

# Function to install Node.js
function Install-NodeJS {
    if (-not (Test-Command "node")) {
        Write-Host "Installing Node.js..." -ForegroundColor Yellow
        choco install nodejs -y
    } else {
        $version = node --version
        Write-Host "Node.js is already installed: $version" -ForegroundColor Green
    }
}

# Function to install Docker Desktop
function Install-DockerDesktop {
    if (-not (Test-Command "docker")) {
        Write-Host "Installing Docker Desktop..." -ForegroundColor Yellow
        choco install docker-desktop -y
        Write-Host "Docker Desktop installed. Please start it manually." -ForegroundColor Yellow
    } else {
        Write-Host "Docker is already installed" -ForegroundColor Green
    }
}

# Function to check Jenkins installation
function Test-Jenkins {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080" -UseBasicParsing -TimeoutSec 5
        Write-Host "Jenkins is running on http://localhost:8080" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "Jenkins is not running or not accessible" -ForegroundColor Red
        return $false
    }
}

# Function to add Jenkins user to docker-users group
function Add-JenkinsToDockerGroup {
    try {
        $jenkinsUser = "jenkins"
        $group = "docker-users"
        
        # Check if Jenkins user exists
        $user = Get-LocalUser -Name $jenkinsUser -ErrorAction SilentlyContinue
        if ($user) {
            Add-LocalGroupMember -Group $group -Member $jenkinsUser -ErrorAction SilentlyContinue
            Write-Host "Added Jenkins user to docker-users group" -ForegroundColor Green
        } else {
            Write-Host "Jenkins user not found. Please add Jenkins service account to docker-users group manually." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Could not add Jenkins to docker-users group. Please do this manually." -ForegroundColor Yellow
    }
}

# Main execution
try {
    Write-Host "Starting Jenkins setup for ProjetPfe..." -ForegroundColor Green
    
    # Install Chocolatey
    Install-Chocolatey
    
    # Install required tools
    Install-DotNetSDK
    Install-NodeJS
    Install-DockerDesktop
    
    # Add Jenkins to docker-users group
    Add-JenkinsToDockerGroup
    
    # Check Jenkins
    $jenkinsRunning = Test-Jenkins
    
    Write-Host "`nSetup Summary:" -ForegroundColor Cyan
    Write-Host "==============" -ForegroundColor Cyan
    
    if (Test-Command "dotnet") {
        $dotnetVersion = dotnet --version
        Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
    }
    
    if (Test-Command "node") {
        $nodeVersion = node --version
        Write-Host "✓ Node.js: $nodeVersion" -ForegroundColor Green
    }
    
    if (Test-Command "npm") {
        $npmVersion = npm --version
        Write-Host "✓ npm: $npmVersion" -ForegroundColor Green
    }
    
    if (Test-Command "docker") {
        Write-Host "✓ Docker: Installed" -ForegroundColor Green
    }
    
    if ($jenkinsRunning) {
        Write-Host "✓ Jenkins: Running" -ForegroundColor Green
    } else {
        Write-Host "✗ Jenkins: Not running" -ForegroundColor Red
    }
    
    Write-Host "`nNext Steps:" -ForegroundColor Yellow
    Write-Host "1. Start Docker Desktop" -ForegroundColor White
    Write-Host "2. Access Jenkins at http://localhost:8080" -ForegroundColor White
    Write-Host "3. Install required Jenkins plugins" -ForegroundColor White
    Write-Host "4. Create pipeline job using Jenkinsfile" -ForegroundColor White
    Write-Host "5. Configure Git repository and credentials" -ForegroundColor White
    
    Write-Host "`nSetup completed!" -ForegroundColor Green
    
} catch {
    Write-Host "Error during setup: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} 