#!/bin/bash

echo "🚀 Starting Mission Management System..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

# Build and start services
echo "📦 Building and starting services..."
docker-compose up --build -d

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 30

# Check service status
echo "🔍 Checking service status..."
docker-compose ps

echo ""
echo "✅ Services are starting up!"
echo ""
echo "🌐 Access your applications:"
echo "   Frontend: http://localhost:3000"
echo "   Backend API: http://localhost:5000"
echo "   API Documentation: http://localhost:5000/swagger"
echo ""
echo "📊 Database:"
echo "   Server: localhost,1433"
echo "   Database: MissionDatabaseTest3"
echo "   Username: sa"
echo "   Password: YourStrong@Passw0rd"
echo ""
echo "📝 Useful commands:"
echo "   View logs: docker-compose logs -f"
echo "   Stop services: docker-compose down"
echo "   Restart services: docker-compose restart" 