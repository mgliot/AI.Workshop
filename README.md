# 🧠 AI.Workshop

## 📌 Overview

**AI.Workshop** is a collection of sample applications and demos showcasing how to build AI-powered solutions with **.NET 10**, **Ollama**, **Aspire 13**, and modern AI frameworks.

It includes:

- **Console-based AI demos** with Ollama LLMs
- **Retrieval-Augmented Generation (RAG)** workflows
- **Vector store integrations** (SQLite-Vec, Qdrant, In-Memory)
- **Microsoft Agent Framework** examples
- **Model Context Protocol (MCP)** server and client implementations
- **Aspire-orchestrated** distributed chat applications
- **Prompty** template-based prompt management

Whether you're exploring **prompt engineering**, **semantic search**, **AI agents**, or **MCP integrations**, this workshop provides ready-to-run examples and reusable components.

---

## 🏗️ Solution Structure

### Console Applications

| Project | Purpose |
|---------|---------|
| **AI.Workshop.ConsoleChat.Ollama** | Local Ollama chat demos with vector stores (SQLite-Vec, Qdrant) |
| **AI.Workshop.ConsoleChat.RAG** | RAG workflows, tool calling, and document search |

### Common Libraries

| Project | Purpose |
|---------|---------|
| **AI.Workshop.VectorStore** | In-memory vector store, PDF/GitHub ingestion pipelines, semantic search |

### Agents

| Project | Purpose |
|---------|---------|
| **AI.Workshop.ConsoleAgent** | Microsoft Agent Framework demos (workflows, tools, multi-agent) |
| **AI.Workshop.WebApiAgent** | Web API with sequential agent workflows (Writer → Editor) |

### Model Context Protocol (MCP)

| Project | Purpose |
|---------|---------|
| **AI.Workshop.MCP.ConsoleServer** | MCP server with stdio transport, tools (Monkey API), resources |
| **AI.Workshop.MCP.ConsoleClient** | MCP client consuming tools from local and GitHub servers |
| **AI.Workshop.MCP.HttpServer** | Minimal MCP HTTP server with ASP.NET Core |

### Aspire (Distributed App)

| Project | Purpose |
|---------|---------|
| **AI.Workshop.ChatApp.AppHost** | Aspire orchestrator (Ollama, Qdrant containers) |
| **AI.Workshop.ChatApp.Web** | Blazor Server chat with PDF ingestion and vector search |
| **AI.Workshop.ChatApp.ServiceDefaults** | Shared Aspire configuration and OpenTelemetry |

---

## 🚀 Features

- **Ollama Integration** – Local LLM inference with llama3.2 and all-minilm embeddings
- **Vector Stores** – SQLite-Vec, Qdrant, and In-Memory implementations
- **RAG Pipelines** – PDF ingestion, chunking, embedding, and semantic search
- **Tool Calling** – Function invocation with AI models
- **Microsoft Agents** – Multi-agent workflows with ChatClientAgent
- **MCP Support** – Model Context Protocol servers and clients for tool extensibility
- **Prompty Templates** – Centralized prompt management with `.prompty` files
- **Aspire Orchestration** – Container management for Ollama and Qdrant
- **OpenTelemetry** – Built-in observability and tracing

---

## 🛠️ Tech Stack

- **.NET 10 / C#**
- **Aspire 13** for distributed app orchestration
- **Ollama** (llama3.2, all-minilm) for local LLM inference
- **Microsoft.Extensions.AI 10.0.1** for unified AI abstractions
- **Microsoft.Agents.AI** for agent workflows
- **ModelContextProtocol 0.4.1** for MCP server/client
- **Semantic Kernel 1.67.1** for vector store connectors
- **Prompty.Core 0.2.3** for prompt template management
- **OllamaSharp 5.4.11** for Ollama API bindings
- **Qdrant** for vector database
- **SQLite-Vec** for embedded vector storage
- **PdfPig** for PDF text extraction
- **Blazor Server** for web UI

---

## 📦 Getting Started

### Option A: Using Dev Container (Recommended)

The easiest way to get started is using the included Dev Container configuration, which provides a fully configured development environment with .NET 10, Ollama, and Qdrant.

#### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

#### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/dedalusmax/AI.Workshop.git
   ```

2. Open in VS Code:
   ```bash
   code AI.Workshop
   ```

3. When prompted, click **"Reopen in Container"** or run the command:
   - `Ctrl+Shift+P` → "Dev Containers: Reopen in Container"

4. Wait for the container to build (first time takes ~5-10 minutes)

5. Run the Aspire AppHost to start all services:
   ```bash
   dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost
   ```
   This will automatically start Ollama, Qdrant, and pull required AI models.

#### Dev Container Features

The Dev Container provides:
- .NET 10 SDK pre-installed
- Docker-in-Docker for Aspire container management
- Git and GitHub CLI

**Note:** Ollama and Qdrant containers are **not** started automatically by the Dev Container. They are managed by .NET Aspire when you run the AppHost project:

```bash
dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost
```

Aspire will automatically:
- Start Ollama container with GPU auto-detection
- Start Qdrant vector database container
- Pull required models (llama3.2, all-minilm)
- Configure all service connections

---

### Option B: Local Development

#### 1️⃣ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Ollama](https://ollama.ai/) installed and running locally
- [Docker](https://www.docker.com/) (for Qdrant and Aspire orchestration)

#### 2️⃣ Install Ollama Models

```bash
ollama pull llama3.2
ollama pull all-minilm
```

#### 3️⃣ Clone the Repository

```bash
git clone https://github.com/dedalusmax/AI.Workshop.git
cd AI.Workshop
```

#### 4️⃣ Build the Solution

```bash
dotnet build AI.Workshop.sln
```

#### 5️⃣ Run Examples

**Console Chat with Ollama:**
```bash
cd AI.Workshop.ConsoleChat.Ollama
dotnet run
```

**RAG Workflow Examples:**
```bash
cd AI.Workshop.ConsoleChat.RAG
dotnet run
```

**Agent Examples:**
```bash
cd Agents/AI.Workshop.ConsoleAgent
dotnet run
```

**MCP Server (requires separate terminal):**
```bash
cd MCP/AI.Workshop.MCP.ConsoleServer
dotnet run
```

**Aspire Chat App:**
```bash
cd Aspire/AI.Workshop.ChatApp.AppHost
dotnet run
```

---

## 📁 Prompty Templates

All system prompts are managed using [Prompty](https://prompty.ai/) `.prompty` files located in each project's `Prompts/` folder:

| Project | Prompts |
|---------|---------|
| ConsoleChat.Ollama | `BookRecommendation`, `ServiceSuggestion`, `DocumentSearch` |
| ConsoleChat.RAG | `GeneralAssistant`, `DocumentSearch`, `DocumentSearchSimple` |
| ConsoleAgent | `AgentSmith`, `WeatherAssistant`, `PersonInfo`, `CroatianTranslator`, `StoryWriter`, `StoryEditor` |
| WebApiAgent | `StoryWriter`, `StoryEditor` |
| MCP.ConsoleClient | `MonkeyAssistant`, `GitHubAssistant` |

Example `.prompty` file:
```yaml
---
name: Document Search Assistant
description: An assistant that answers questions based on retrieved documents
model:
  api: chat
  configuration:
    type: ollama
    model: llama3.2
---
system:
You are an assistant who answers questions about information you retrieve.
Use the search tool to find relevant information.

user:
{{question}}
```

---

## 🔧 Configuration

### Ollama Endpoint
Default: `http://localhost:11434/`

Configure in code or environment variables as needed.

### GPU Acceleration (Aspire)

The Aspire AppHost automatically detects and configures GPU acceleration:

**⚠️ Windows + Docker Desktop Limitation:**
Docker Desktop runs Linux containers in WSL2. GPU passthrough only works for **NVIDIA GPUs** with the NVIDIA Container Toolkit. AMD/Intel Vulkan drivers cannot be passed through to Linux containers.

| Host OS | GPU Vendor | Container Support | Alternative |
|---------|------------|-------------------|-------------|
| Windows | NVIDIA | ✅ Works (via Container Toolkit) | - |
| Windows | AMD | ❌ CPU fallback | Run Ollama natively with `OLLAMA_VULKAN=1` |
| Windows | Intel | ❌ CPU fallback | Run Ollama natively with `OLLAMA_VULKAN=1` |
| Linux | NVIDIA | ✅ Works (via Container Toolkit) | - |
| Linux | AMD | ✅ Works (ROCm image) | - |
| Linux | Intel | ✅ Works (device passthrough) | - |

**For Windows AMD/Intel users wanting GPU acceleration:**
```bash
# Option 1: Run Ollama natively (outside Docker)
$env:OLLAMA_VULKAN="1"
ollama serve

# Then run Aspire without the Ollama container (configure to use external Ollama)
```

**Auto-detection:**
- Windows: Uses WMI `Win32_VideoController` queries
- Priority: NVIDIA → AMD → Intel

Set the `GPU_VENDOR` environment variable to override auto-detection:

| Value | Description |
|-------|-------------|
| `nvidia` | NVIDIA GPU (requires NVIDIA Container Toolkit) |
| `amd` | AMD GPU - Linux: ROCm, Windows: CPU fallback |
| `intel` | Intel GPU - Linux: device passthrough, Windows: CPU fallback |
| `cpu` or `none` | Disable GPU, use CPU only |
| *(not set)* | Auto-detect (NVIDIA → AMD → Intel) |

**Example:**
```bash
# Force CPU-only mode
GPU_VENDOR=cpu dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost
```

**Requirements by GPU vendor:**
- **NVIDIA**: Install [NVIDIA Container Toolkit](https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/)
- **AMD (Linux)**: Install ROCm drivers, add user to `render` and `video` groups
- **Intel (Linux)**: Install Intel compute runtime and level-zero drivers

### Qdrant (for Aspire)
Automatically provisioned by Aspire AppHost with persistent data volume.

### Vector Store Selection
In `AI.Workshop.ChatApp.Web`, set `VECTOR_STORE` to either `Qdrant` or `Sqlite`.

---

## 📚 Resources

- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [Ollama Documentation](https://ollama.ai/)
- [Prompty Documentation](https://prompty.ai/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Microsoft Agent Framework](https://github.com/microsoft/Agents-for-net)

---

## 📄 License

This project is for educational and demonstration purposes.
