@echo off
echo 🛑 Stopping Mission Management System...

REM Stop all services
docker-compose down

echo ✅ All services have been stopped!
echo.
echo 📝 To start again, run: start.bat
echo 📝 To remove all data and start fresh, run: docker-compose down -v && start.bat

pause 