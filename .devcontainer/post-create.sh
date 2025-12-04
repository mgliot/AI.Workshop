#!/bin/bash
set -e

echo "🚀 Running post-create setup..."

# Navigate to workspace
cd /workspace

# Restore NuGet packages
echo "📦 Restoring NuGet packages..."
dotnet restore AI.Workshop.sln || true

# Trust HTTPS certificates
echo "🔐 Trusting HTTPS dev certificates..."
dotnet dev-certs https --trust 2>/dev/null || true

echo "✅ Post-create setup complete!"
echo ""
echo "📝 Available commands:"
echo "  dotnet build AI.Workshop.sln                                    - Build the solution"
echo "  dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost         - Run Aspire AppHost (starts Ollama & Qdrant)"
echo ""
echo "ℹ️  Ollama and Qdrant containers are managed by .NET Aspire."
echo "   They will start automatically when you run the AppHost project."
