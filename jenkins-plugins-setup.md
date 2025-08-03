# Jenkins Plugins Setup for ProjetPfe

## Required Plugins to Install

1. **Pipeline** (workflow-aggregator)
2. **Git** (git)
3. **Docker Pipeline** (docker-workflow)
4. **HTML Publisher** (htmlpublisher)
5. **Test Results Aggregator** (test-results-aggregator)
6. **Blue Ocean** (blueocean) - Optional but recommended

## Installation Steps

1. **Access Jenkins**
   - Open browser and go to `http://localhost:8080`

2. **Install Plugins**
   - Go to **Manage Jenkins** > **Manage Plugins**
   - Click on **Available** tab
   - Search for each plugin and check the box
   - Click **Install without restart**

3. **Restart Jenkins**
   - After installation, restart Jenkins when prompted

## Configure Global Tools

1. **Go to Manage Jenkins > Global Tool Configuration**

2. **Configure Git**
   - Find **Git installations**
   - Add Git installation
   - Name: `Default`
   - Path to Git executable: `git` (or full path if needed)

3. **Configure NodeJS** (for Angular frontend)
   - Find **NodeJS installations**
   - Add NodeJS installation
   - Name: `NodeJS 18`
   - Install automatically: Check this
   - Version: Select latest LTS version

4. **Save Configuration**

## Configure Credentials

1. **Go to Manage Jenkins > Manage Credentials**

2. **Add Git Repository Credentials** (if needed)
   - Click **System** > **Global credentials** > **Add Credentials**
   - Kind: **Username with password**
   - Scope: **Global**
   - Username: Your Git username
   - Password: Your Git password/token
   - ID: `git-credentials`
   - Description: `Git Repository Credentials`

## Create Pipeline Job

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

## Test the Pipeline

1. **Run Pipeline Manually**
   - Go to your pipeline job
   - Click **Build Now**

2. **Monitor Build**
   - Click on the build number to see progress
   - Check console output for any errors

## Troubleshooting

### Common Issues

1. **Git not found**
   - Ensure Git is installed and in PATH
   - Restart Jenkins after installing Git

2. **Node.js not found**
   - Install Node.js on the Jenkins server
   - Configure NodeJS tool in Jenkins

3. **Docker not accessible**
   - Ensure Docker Desktop is running
   - Add Jenkins user to docker-users group

4. **Permission issues**
   - Run Jenkins as administrator
   - Or configure proper permissions

### Debug Commands

```powershell
# Check if tools are accessible
git --version
node --version
npm --version
docker --version
dotnet --version

# Check Jenkins service
Get-Service -Name Jenkins

# Check Jenkins logs
Get-Content "C:\Program Files\Jenkins\jenkins.err.log"
```

## Next Steps

After setting up the plugins and pipeline:

1. **Test the pipeline** with a simple build
2. **Configure environment variables** if needed
3. **Set up notifications** (email, Slack, etc.)
4. **Configure quality gates** (code coverage, test results)
5. **Set up deployment environments**

## Pipeline Features

The pipeline will automatically:
- ✅ Build .NET backend
- ✅ Build Angular frontend  
- ✅ Run tests
- ✅ Create Docker images
- ✅ Deploy with Docker Compose
- ✅ Generate test reports
- ✅ Clean up resources 