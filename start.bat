@echo off
echo 🚀 Starting Mission Management System...

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

REM Build and start services
echo 📦 Building and starting services...
docker-compose up --build -d

REM Wait for services to be ready
echo ⏳ Waiting for services to be ready...
timeout /t 30 /nobreak >nul

REM Check service status
echo 🔍 Checking service status...
docker-compose ps

echo.
echo ✅ Services are starting up!
echo.
echo 🌐 Access your applications:
echo    Frontend: http://localhost:3000
echo    Backend API: http://localhost:5000
echo    API Documentation: http://localhost:5000/swagger
echo.
echo 📊 Database:
echo    Server: localhost,1433
echo    Database: MissionDatabaseTest3
echo    Username: sa
echo    Password: YourStrong@Passw0rd
echo.
echo 📝 Useful commands:
echo    View logs: docker-compose logs -f
echo    Stop services: docker-compose down
echo    Restart services: docker-compose restart

pause 