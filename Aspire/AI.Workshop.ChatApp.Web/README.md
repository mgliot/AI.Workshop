# AI Workshop Chat Web Application

A Blazor Server application demonstrating RAG (Retrieval-Augmented Generation) chat with PDF documents using local AI models powered by Ollama and .NET Aspire.

## Features

- **Local AI Models**: Uses Ollama for both chat (`llama3.2`) and embeddings (`all-minilm`) - no cloud API keys required
- **RAG Pipeline**: Search and chat with your PDF documents using semantic search
- **Vector Storage**: Supports both Qdrant and SQLite for vector storage
- **Aspire Orchestration**: Full .NET Aspire 13 integration for container orchestration
- **GPU Acceleration**: Automatic GPU detection for NVIDIA (CUDA), AMD and Intel (Vulkan on Windows, ROCm on Linux)

## Technology Stack

| Component | Version |
|-----------|---------|
| .NET | 10.0 |
| Aspire | 13.0.0 |
| Ollama | Latest |
| OllamaSharp | 5.4.11 |
| Microsoft.Extensions.AI | 10.0.1 |
| Qdrant | Latest |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/) (for Ollama and Qdrant containers)
- Optional: NVIDIA/AMD/Intel GPU for accelerated inference

## Project Structure

```
Aspire/
├── AI.Workshop.ChatApp.AppHost/     # Aspire orchestration host
├── AI.Workshop.ChatApp.Web/         # Blazor Server web application
└── AI.Workshop.ChatApp.ServiceDefaults/  # Shared service configuration
```

## Running the Application

### Using Visual Studio

1. Open `AI.Workshop.sln` in Visual Studio
2. Set `AI.Workshop.ChatApp.AppHost` as the startup project
3. Press `F5` or click "Start"

### Using Command Line

```bash
dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost
```

### Using Visual Studio Code

1. Open the project folder in VS Code
2. Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
3. Open `Program.cs` in the AppHost project
4. Press `F5` to start debugging

## Configuration

### Vector Store

Set the `VECTOR_STORE` environment variable to choose your vector database:

- `Qdrant` (default) - Uses Qdrant container for vector storage
- `Sqlite` - Uses local SQLite database with vector extensions

### GPU Acceleration

The Aspire AppHost automatically detects GPU vendor and configures acceleration.

**⚠️ Windows + Docker Limitation:**
Docker Desktop uses WSL2 (Linux VM). Only **NVIDIA GPUs** can be passed through to containers. AMD/Intel GPUs will fall back to CPU mode in containers.

| Host | GPU | Works in Container? |
|------|-----|---------------------|
| Windows | NVIDIA | ✅ Yes |
| Windows | AMD/Intel | ❌ No (use native Ollama with `OLLAMA_VULKAN=1`) |
| Linux | Any | ✅ Yes |

Set the `GPU_VENDOR` environment variable to override:

- `nvidia` - NVIDIA GPU (requires NVIDIA Container Toolkit)
- `amd` - AMD GPU (Linux: ROCm, Windows: CPU fallback)
- `intel` - Intel GPU (Linux: passthrough, Windows: CPU fallback)
- `cpu` or `none` - CPU-only mode

## Adding Your Documents

Place PDF files in the `wwwroot/Data` folder. The application will automatically:

1. Extract text from PDFs using PdfPig
2. Chunk text into semantic paragraphs
3. Generate embeddings using the `all-minilm` model
4. Store vectors in the configured vector database

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Blazor Web    │────▶│     Ollama      │────▶│   llama3.2      │
│   Application   │     │   (Container)   │     │   all-minilm    │
└────────┬────────┘     └─────────────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐
│  Qdrant/SQLite  │
│  Vector Store   │
└─────────────────┘
```

## Troubleshooting

### Trust the localhost certificate

If you encounter certificate errors on first run:

```bash
dotnet dev-certs https --trust
```

See [Troubleshoot untrusted localhost certificate](https://learn.microsoft.com/dotnet/aspire/troubleshooting/untrusted-localhost-certificate) for more details.

### Docker/Ollama Issues

Ensure Docker Desktop 4.41.1+ is installed. Earlier versions have compatibility issues with Ollama containers.

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Ollama](https://ollama.ai/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/)
- [Qdrant Vector Database](https://qdrant.tech/)
