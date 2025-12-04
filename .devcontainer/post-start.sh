#!/bin/bash
set -e

echo "🔄 Running post-start setup..."

# Check if Ollama is available
echo "🤖 Checking Ollama availability..."

max_attempts=10
attempt=0
ollama_available=false

until curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ $attempt -ge $max_attempts ]; then
        echo ""
        echo "⚠️  Ollama is not running!"
        echo ""
        echo "To start Ollama, run one of these commands on your host machine:"
        echo "  Option 1: docker run -d -p 11434:11434 --name ollama ollama/ollama"
        echo "  Option 2: Start from docker-compose: docker-compose --profile services up -d ollama"
        echo ""
        break
    fi
    echo "Waiting for Ollama... (attempt $attempt/$max_attempts)"
    sleep 2
done

if curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; then
    ollama_available=true
    echo "✅ Ollama is available"
    
    # Check if models are already pulled
    models=$(curl -sf http://localhost:11434/api/tags 2>/dev/null || echo '{"models":[]}')
    
    if ! echo "$models" | jq -e '.models[] | select(.name | startswith("llama3.2"))' > /dev/null 2>&1; then
        echo "📥 Pulling llama3.2 model (this may take a few minutes)..."
        curl -X POST http://localhost:11434/api/pull -d '{"name": "llama3.2"}' &
    else
        echo "✅ llama3.2 model already available"
    fi
    
    if ! echo "$models" | jq -e '.models[] | select(.name | startswith("all-minilm"))' > /dev/null 2>&1; then
        echo "📥 Pulling all-minilm model..."
        curl -X POST http://localhost:11434/api/pull -d '{"name": "all-minilm"}' &
    else
        echo "✅ all-minilm model already available"
    fi
    
    # Wait for background pulls
    wait
fi

# Check if Qdrant is available
echo ""
echo "🔍 Checking Qdrant availability..."
if curl -sf http://localhost:6333/readyz > /dev/null 2>&1; then
    echo "✅ Qdrant is available"
else
    echo ""
    echo "⚠️  Qdrant is not running!"
    echo ""
    echo "To start Qdrant, run one of these commands on your host machine:"
    echo "  Option 1: docker run -d -p 6333:6333 -p 6334:6334 --name qdrant qdrant/qdrant"
    echo "  Option 2: Start from docker-compose: docker-compose --profile services up -d qdrant"
    echo ""
fi

echo ""
echo "✅ Post-start setup complete!"
echo ""
echo "🚀 Ready to develop! Try:"
echo "  cd /workspace"
echo "  dotnet build AI.Workshop.sln"
echo ""
if [ "$ollama_available" = false ]; then
    echo "⚠️  Note: Start Ollama and Qdrant before running AI examples"
fi
