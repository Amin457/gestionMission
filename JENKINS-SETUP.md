# Jenkins Pipeline Setup Guide

## Prerequisites

### 1. Jenkins Installation
- Jenkins LTS installed on Windows
- Jenkins running on port 8080 (default)
- Jenkins accessible at `http://localhost:8080`

### 2. Required Jenkins Plugins
Install the following plugins in Jenkins:
- **Pipeline** (workflow-aggregator)
- **Git** (git)
- **Docker Pipeline** (docker-workflow)
- **HTML Publisher** (htmlpublisher)
- **Test Results Aggregator** (test-results-aggregator)
- **Blue Ocean** (blueocean) - Optional but recommended

### 3. System Requirements
- .NET 8.0 SDK
- Node.js 18+ and npm
- Docker Desktop
- Git

## Setup Steps

### Step 1: Install Required Tools on Jenkins Server

1. **Install .NET 8.0 SDK**
   ```powershell
   # Download and install from Microsoft
   # https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Install Node.js**
   ```powershell
   # Download and install from nodejs.org
   # https://nodejs.org/en/download/
   ```

3. **Install Docker Desktop**
   ```powershell
   # Download and install from Docker
   # https://www.docker.com/products/docker-desktop/
   ```

4. **Verify installations**
   ```powershell
   dotnet --version
   node --version
   npm --version
   docker --version
   ```

### Step 2: Configure Jenkins

1. **Access Jenkins**
   - Open browser and go to `http://localhost:8080`
   - Complete initial setup if not done

2. **Install Required Plugins**
   - Go to **Manage Jenkins** > **Manage Plugins**
   - Install the plugins listed above

3. **Configure Global Tools**
   - Go to **Manage Jenkins** > **Global Tool Configuration**
   - Configure:
     - **Git**: Set path to git executable
     - **NodeJS**: Set Node.js installation
     - **Docker**: Ensure Docker is accessible

4. **Configure Credentials** (if needed)
   - Go to **Manage Jenkins** > **Manage Credentials**
   - Add any required credentials for your Git repository

### Step 3: Create Jenkins Pipeline Job

1. **Create New Job**
   - Click **New Item**
   - Enter job name: `projetpfe-pipeline`
   - Select **Pipeline**
   - Click **OK**

2. **Configure Pipeline**
   - **Description**: `CI/CD Pipeline for ProjetPfe - Mission Management System`
   - **Build Triggers**: 
     - Check **Poll SCM**
     - Schedule: `H/5 * * * *` (every 5 minutes)
   - **Pipeline**: 
     - Definition: **Pipeline script from SCM**
     - SCM: **Git**
     - Repository URL: Your Git repository URL
     - Credentials: Select your Git credentials
     - Branch Specifier: `*/main` (or your main branch)
     - Script Path: `Jenkinsfile`

3. **Save Configuration**

### Step 4: Configure Environment Variables

Add these environment variables in Jenkins:

1. **Go to Manage Jenkins > Configure System**
2. **Add Environment Variables**:
   ```
   DOCKER_REGISTRY=your-registry.com
   DOCKER_USERNAME=your-username
   DOCKER_PASSWORD=your-password
   ```

### Step 5: Test the Pipeline

1. **Run Pipeline Manually**
   - Go to your pipeline job
   - Click **Build with Parameters**
   - Set parameters:
     - BRANCH: `main`
     - SKIP_TESTS: `false`
     - DEPLOY: `true`

2. **Monitor Build**
   - Click on the build number to see progress
   - Check console output for any errors

## Pipeline Stages

The pipeline includes the following stages:

1. **Checkout**: Clones the repository
2. **Build Backend**: Builds .NET backend
3. **Test Backend**: Runs backend unit tests
4. **Build Frontend**: Builds Angular frontend
5. **Test Frontend**: Runs frontend tests
6. **Build Docker Images**: Creates Docker images
7. **Test Docker Images**: Tests containers
8. **Deploy**: Deploys with Docker Compose

## Troubleshooting

### Common Issues

1. **Docker not accessible**
   ```powershell
   # Ensure Docker Desktop is running
   # Add Jenkins user to docker-users group
   ```

2. **Node.js not found**
   ```powershell
   # Add Node.js to PATH
   # Restart Jenkins service
   ```

3. **.NET not found**
   ```powershell
   # Add .NET to PATH
   # Restart Jenkins service
   ```

4. **Permission issues**
   ```powershell
   # Run Jenkins as administrator
   # Or configure proper permissions
   ```

### Debug Commands

```powershell
# Check Jenkins service
Get-Service -Name Jenkins

# Check Jenkins logs
Get-Content "C:\Program Files\Jenkins\jenkins.err.log"

# Restart Jenkins
Restart-Service -Name Jenkins
```

## Customization

### Modify Pipeline for Your Needs

1. **Change Docker Registry**
   - Update `DOCKER_REGISTRY` in Jenkinsfile
   - Uncomment push commands when ready

2. **Add More Tests**
   - Add integration tests
   - Add E2E tests
   - Add security scans

3. **Add Notifications**
   - Email notifications
   - Slack notifications
   - Teams notifications

4. **Add Quality Gates**
   - Code coverage thresholds
   - Test pass rates
   - Security scan results

## Security Considerations

1. **Credentials Management**
   - Use Jenkins credentials store
   - Never hardcode passwords
   - Rotate credentials regularly

2. **Docker Security**
   - Scan images for vulnerabilities
   - Use minimal base images
   - Implement image signing

3. **Network Security**
   - Use HTTPS for Jenkins
   - Configure firewall rules
   - Implement access controls

## Monitoring and Maintenance

1. **Regular Maintenance**
   - Clean up old builds
   - Update Jenkins and plugins
   - Monitor disk space

2. **Backup Strategy**
   - Backup Jenkins configuration
   - Backup build artifacts
   - Document configuration

3. **Performance Monitoring**
   - Monitor build times
   - Monitor resource usage
   - Optimize pipeline stages 