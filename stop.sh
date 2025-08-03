#!/bin/bash

echo "🛑 Stopping Mission Management System..."

# Stop all services
docker-compose down

echo "✅ All services have been stopped!"

echo ""
echo "📝 To start again, run: ./start.sh"
echo "📝 To remove all data and start fresh, run: docker-compose down -v && ./start.sh" 