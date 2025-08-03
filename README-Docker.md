# Docker Setup for Mission Management System

This project includes Docker configuration for running the entire application stack including frontend, backend, and SQL Server database.

## Prerequisites

- Docker Desktop installed and running
- At least 4GB of available RAM
- At least 10GB of available disk space

## Quick Start

1. **Clone and navigate to the project directory**
   ```bash
   cd ProjetPfe
   ```

2. **Build and start all services**
   ```bash
   docker-compose up --build
   ```

3. **Access the applications**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - SQL Server: localhost:1433

## Services Overview

### Frontend (Angular)
- **Port**: 3000
- **Technology**: Angular 17 with Nginx
- **URL**: http://localhost:3000

### Backend (ASP.NET Core)
- **Port**: 5000
- **Technology**: .NET 8.0
- **URL**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger

### Database (SQL Server)
- **Port**: 1433
- **Technology**: SQL Server 2022 Express
- **Database**: MissionDatabaseTest3
- **Username**: sa
- **Password**: YourStrong@Passw0rd

## Docker Commands

### Start services
```bash
docker-compose up
```

### Start services in background
```bash
docker-compose up -d
```

### Build and start services
```bash
docker-compose up --build
```

### Stop services
```bash
docker-compose down
```

### Stop services and remove volumes
```bash
docker-compose down -v
```

### View logs
```bash
docker-compose logs
```

### View logs for specific service
```bash
docker-compose logs backend
docker-compose logs frontend
docker-compose logs sqlserver
```

### Access container shell
```bash
docker-compose exec backend bash
docker-compose exec frontend sh
docker-compose exec sqlserver bash
```

## Database Connection

### From Backend
The backend is configured to connect to the SQL Server container using the following connection string:
```
Server=sqlserver;Database=MissionDatabaseTest3;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true
```

### From External Tools
You can connect to the database using any SQL Server client:
- **Server**: localhost,1433
- **Database**: MissionDatabaseTest3
- **Username**: sa
- **Password**: YourStrong@Passw0rd

## File Structure

```
ProjetPfe/
├── docker-compose.yml          # Main Docker Compose file
├── database/
│   └── init.sql               # Database initialization script
├── gestionMissionBack/
│   ├── Dockerfile             # Backend Dockerfile
│   └── .dockerignore          # Backend Docker ignore file
└── gestionMissionFront/
    ├── Dockerfile             # Frontend Dockerfile
    ├── nginx.conf             # Nginx configuration
    └── .dockerignore          # Frontend Docker ignore file
```

## Environment Variables

### Backend Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development
- `ConnectionStrings__DefaultConnection`: Database connection string
- `ASPNETCORE_URLS`: http://+:80

### SQL Server Environment Variables
- `ACCEPT_EULA`: Y
- `SA_PASSWORD`: YourStrong@Passw0rd
- `MSSQL_PID`: Express

## Troubleshooting

### Port Conflicts
If you get port conflicts, you can modify the ports in `docker-compose.yml`:
```yaml
ports:
  - "5001:80"  # Change 5000 to 5001
```

### Database Connection Issues
1. Ensure SQL Server container is healthy:
   ```bash
   docker-compose ps
   ```

2. Check SQL Server logs:
   ```bash
   docker-compose logs sqlserver
   ```

3. Wait for database initialization to complete (check logs for "Database initialization completed successfully!")

### Frontend Build Issues
1. Clear Docker cache:
   ```bash
   docker system prune -a
   ```

2. Rebuild frontend:
   ```bash
   docker-compose build frontend
   ```

### Backend Build Issues
1. Ensure all project references are correct
2. Check if all required NuGet packages are installed
3. Rebuild backend:
   ```bash
   docker-compose build backend
   ```

## Development Workflow

1. **Start the stack**: `docker-compose up -d`
2. **Make code changes** in your IDE
3. **Rebuild specific service**: `docker-compose build [service-name]`
4. **Restart specific service**: `docker-compose restart [service-name]`
5. **View logs**: `docker-compose logs -f [service-name]`

## Production Considerations

For production deployment, consider:

1. **Security**:
   - Change default passwords
   - Use environment variables for sensitive data
   - Enable HTTPS
   - Configure proper firewall rules

2. **Performance**:
   - Use production-grade SQL Server
   - Configure proper resource limits
   - Enable caching layers

3. **Monitoring**:
   - Add health checks
   - Configure logging
   - Set up monitoring tools

4. **Backup**:
   - Configure database backups
   - Backup uploaded files
   - Document recovery procedures 