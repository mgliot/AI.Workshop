#!/bin/bash
set -e

echo "🔄 Running post-start setup..."

# Verify Docker socket is accessible for Aspire
echo "🐳 Checking Docker availability..."
if docker info > /dev/null 2>&1; then
    echo "✅ Docker is available"
else
    echo "⚠️  Docker socket not accessible. Aspire won't be able to manage containers."
    echo "   Ensure /var/run/docker.sock is mounted correctly."
fi

echo ""
echo "✅ Post-start setup complete!"
echo ""
echo "🚀 Ready to develop! Run the Aspire AppHost to start all services:"
echo "  cd /workspace"
echo "  dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost"
echo ""
echo "ℹ️  Aspire will automatically start and manage:"
echo "   - Ollama container (with model downloads)"
echo "   - Qdrant vector database"
echo "   - Web application"
