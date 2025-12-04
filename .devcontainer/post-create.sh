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

# Check and start Ollama container if not running
echo "🐳 Checking Ollama container..."
if ! curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "Starting Ollama container..."
    docker-compose -f /workspace/.devcontainer/docker-compose.yml --profile services up -d ollama || true
fi

# Check and start Qdrant container if not running
echo "🐳 Checking Qdrant container..."
if ! curl -sf http://localhost:6333/readyz > /dev/null 2>&1; then
    echo "Starting Qdrant container..."
    docker-compose -f /workspace/.devcontainer/docker-compose.yml --profile services up -d qdrant || true
fi

# Wait for Ollama to be ready
echo "⏳ Waiting for Ollama to be ready..."
max_attempts=30
attempt=0
until curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ $attempt -ge $max_attempts ]; then
        echo "⚠️ Ollama not ready after $max_attempts attempts, continuing anyway..."
        break
    fi
    echo "Waiting for Ollama... (attempt $attempt/$max_attempts)"
    sleep 2
done

# Wait for Qdrant to be ready
echo "⏳ Waiting for Qdrant to be ready..."
attempt=0
until curl -sf http://localhost:6333/readyz > /dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ $attempt -ge $max_attempts ]; then
        echo "⚠️ Qdrant not ready after $max_attempts attempts, continuing anyway..."
        break
    fi
    echo "Waiting for Qdrant... (attempt $attempt/$max_attempts)"
    sleep 2
done

# Pull required Ollama models
echo "🤖 Pulling Ollama models..."
if curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "Pulling llama3.2..."
    curl -X POST http://localhost:11434/api/pull -d '{"name": "llama3.2"}' || true

    echo "Pulling all-minilm (embeddings)..."
    curl -X POST http://localhost:11434/api/pull -d '{"name": "all-minilm"}' || true
else
    echo "⚠️ Ollama not available, skipping model pull. Run post-start.sh manually later."
fi

echo "✅ Post-create setup complete!"
echo ""
echo "📝 Available commands:"
echo "  dotnet build AI.Workshop.sln    - Build the solution"
echo "  dotnet run --project <project>  - Run a specific project"
echo ""
echo "🔗 Service endpoints:"
echo "  Ollama API: http://localhost:11434"
echo "  Qdrant HTTP: http://localhost:6333"
echo "  Qdrant gRPC: http://localhost:6334"
