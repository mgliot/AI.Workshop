# 🧠 AI.Workshop

## 📌 Overview

**AI.Workshop** is a collection of sample applications and demos showcasing how to build AI-powered solutions with **.NET 10**, **Ollama**, **Aspire 13**, and modern AI frameworks.

It includes:

- **Console-based AI demos** with Ollama LLMs
- **Retrieval-Augmented Generation (RAG)** workflows
- **Vector store integrations** (SQLite-Vec, Qdrant, In-Memory)
- **AI Guardrails** – Input/output validation, prompt injection detection, PII filtering
- **Microsoft Agent Framework** examples
- **Model Context Protocol (MCP)** server and client implementations
- **Aspire-orchestrated** distributed chat applications
- **Prompty** template-based prompt management

Whether you're exploring **prompt engineering**, **semantic search**, **AI agents**, or **MCP integrations**, this workshop provides ready-to-run examples and reusable components.

> 📐 **Architecture Documentation**: See the [`docs/`](./docs/) folder for detailed architecture diagrams and technical documentation for each project.

---

## 🏗️ Solution Structure

### Shared Data

| Folder | Purpose |
|--------|---------|
| **Data/** | Shared PDF documents for RAG demos (copied to project outputs at build time) |

### Console Applications

| Project | Purpose | Docs |
|---------|---------|------|
| **AI.Workshop.Console.VectorDemos** | Compare vector backends: In-Memory, SQLite-Vec, Qdrant | [📐](./docs/AI.Workshop.Console.VectorDemos.md) |
| **AI.Workshop.Console.AgentChat** | Agent progression: Chat → Tools → RAG → Summarization | [📐](./docs/AI.Workshop.Console.AgentChat.md) |
| **AI.Workshop.Console.Agents** | Microsoft Agent Framework demos (workflows, multi-agent) | [📐](./docs/AI.Workshop.Console.Agents.md) |

### Web Applications

| Project | Purpose | Docs |
|---------|---------|------|
| **AI.Workshop.WebApi.Agents** | Web API with sequential agent workflows (Writer → Editor) | [📐](./docs/AI.Workshop.WebApi.Agents.md) |

### Common Libraries

| Project | Purpose | Docs |
|---------|---------|------|
| **AI.Workshop.Common** | Shared constants, Prompty helper, DI extensions, TOON, caching, health checks | [📐](./docs/AI.Workshop.Common.md) |
| **AI.Workshop.Guardrails** | AI guardrails middleware – input/output validation, prompt injection, PII, toxicity, rate limiting, topic restriction, LLM moderation, telemetry | [📐](./docs/AI.Workshop.Guardrails.md) |
| **AI.Workshop.VectorStore** | In-memory vector store, PDF/GitHub ingestion pipelines, semantic search | [📐](./docs/AI.Workshop.VectorStore.md) |
| **AI.Workshop.Tests** | 198 unit tests for Common, Guardrails, and VectorStore (xUnit, FluentAssertions) | - |

### Model Context Protocol (MCP)

| Project | Purpose | Docs |
|---------|---------|------|
| **AI.Workshop.MCP.ConsoleServer** | MCP server with stdio transport, tools (Monkey API), resources | [📐](./docs/AI.Workshop.MCP.md) |
| **AI.Workshop.MCP.ConsoleClient** | MCP client consuming tools from local and GitHub servers | [📐](./docs/AI.Workshop.MCP.md) |
| **AI.Workshop.MCP.HttpServer** | Minimal MCP HTTP server with ASP.NET Core | [📐](./docs/AI.Workshop.MCP.md) |

### Aspire (Distributed App)

| Project | Purpose | Docs |
|---------|---------|------|
| **AI.Workshop.ChatApp.AppHost** | Aspire orchestrator (Ollama, Qdrant containers) | [📐](./docs/AI.Workshop.ChatApp.md) |
| **AI.Workshop.ChatApp.Web** | Full-featured Blazor chat: agent selection, RAG, TOON, guardrails, token tracking, stats bar, theme toggle | [📐](./docs/AI.Workshop.ChatApp.md) |
| **AI.Workshop.ChatApp.ServiceDefaults** | Shared Aspire configuration and OpenTelemetry | [📐](./docs/AI.Workshop.ChatApp.md) |

---

## 🚀 Features

- **Ollama Integration** – Local LLM inference with llama3.2 and all-minilm embeddings
- **Vector Stores** – SQLite-Vec, Qdrant, and In-Memory implementations
- **RAG Pipelines** – PDF ingestion, chunking, embedding, and semantic search
- **AI Guardrails** – Middleware for input/output validation:
  - Prompt injection detection (24+ attack patterns)
  - PII detection (SSN, credit cards, emails, phones, IP addresses)
  - Toxicity filtering (violence, self-harm, illegal activity)
  - Rate limiting with sliding window algorithm
  - Topic restriction with semantic similarity matching
  - LLM-based content moderation
  - Custom keyword/pattern blocking
  - Configurable actions (Block, LogOnly, Redact)
  - Built-in telemetry and metrics collection
- **Tool Calling** – Function invocation with AI models
- **Microsoft Agents** – Multi-agent workflows with ChatClientAgent
- **MCP Support** – Model Context Protocol servers and clients for tool extensibility
- **Prompty Templates** – Centralized prompt management with `.prompty` files
- **Aspire Orchestration** – Container management for Ollama and Qdrant
- **OpenTelemetry** – Built-in observability and tracing
- **Embedding Caching** – In-memory and distributed caching for embeddings
- **TOON Support** – Token-Oriented Object Notation for 30-60% token savings
- **Interactive Feature Toggles** – Runtime enable/disable for Guardrails and TOON in console and web
- **Token Usage Tracking** – Per-response and session totals for LLM token consumption (always displayed)
- **Ollama Health Check** – Connection validation with retry logic before starting apps

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
- **ToonSharp 1.0.0** for TOON serialization
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

**Vector Store Demos:**
```bash
cd AI.Workshop.Console.VectorDemos
dotnet run
```

**Interactive Agent Chat (RAG):**
```bash
cd AI.Workshop.Console.AgentChat
dotnet run
```

**Agent Framework Examples:**
```bash
cd AI.Workshop.Console.Agents
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

**Run Unit Tests:**
```bash
dotnet test AI.Workshop.Tests
```

---

## 🛡️ AI Guardrails

The `AI.Workshop.Guardrails` library provides middleware for validating AI inputs and outputs using the `Microsoft.Extensions.AI` pipeline pattern.

### Validators

| Validator | Description | Priority |
|-----------|-------------|----------|
| **Rate Limiting** | Sliding window rate limiting per client | 5 |
| **Input Length** | Enforces maximum input character limits | 10 |
| **Prompt Injection** | Detects 24+ injection attack patterns | 20 |
| **PII Detection** | Blocks SSN, credit cards, emails, phones, IP addresses, passports | 30 |
| **Topic Restriction** | Keyword or semantic similarity matching | 35 |
| **Toxicity Filtering** | Blocks violence, self-harm, illegal activity, harassment | 40 |
| **Custom Keywords** | Block custom keywords or regex patterns | 50 |
| **LLM Moderation** | LLM-based content moderation for nuanced detection | 100 |

### Basic Usage

```csharp
using AI.Workshop.Guardrails;
using Microsoft.Extensions.AI;

// Option 1: ChatClientBuilder pipeline
IChatClient client = new ChatClientBuilder(ollamaClient)
    .UseGuardrails(options =>
    {
        options.EnablePromptInjectionDetection = true;
        options.EnablePiiDetection = true;
        options.EnableToxicityFiltering = true;
        options.BlockedKeywords = ["confidential", "secret"];
        options.DefaultAction = GuardrailAction.Block;
    }, onViolation: result =>
    {
        Console.WriteLine($"Blocked: {result.ViolationType} - {result.ViolationMessage}");
    })
    .UseFunctionInvocation()
    .Build();

// Option 2: Wrap existing client
IChatClient guardedClient = existingClient.WithGuardrails(options =>
{
    options.MaxInputLength = 5000;
    options.EnablePromptInjectionDetection = true;
});

// Option 3: Dependency Injection
builder.Services.AddGuardrails(options =>
{
    options.EnablePiiDetection = true;
});

builder.Services.AddChatClient(services => ollamaClient.AsIChatClient())
    .UseGuardrails();
```

### Advanced Features

```csharp
// Rate limiting per client
builder.Services.AddAdvancedGuardrails(options =>
{
    options.EnableRateLimiting = true;
    options.RateLimitMaxRequests = 60;
    options.RateLimitWindowSeconds = 60;
    options.RateLimitClientId = "user-123";  // Per-user tracking
});

// Topic restriction with semantic similarity
builder.Services.AddAdvancedGuardrails(options =>
{
    options.EnableTopicRestriction = true;
    options.AllowedTopics = ["programming", "software development", "technology"];
    options.TopicSimilarityThreshold = 0.5;  // 0.0 to 1.0
});

// LLM-based moderation (requires IChatClient in DI)
builder.Services.AddAdvancedGuardrails(options =>
{
    options.EnableLlmModeration = true;
    options.LlmModerationFailureAction = GuardrailAction.LogOnly;
});

// Access metrics
var metrics = app.Services.GetRequiredService<GuardrailsMetrics>();
var summary = metrics.GetSummary();
Console.WriteLine($"Block rate: {summary.BlockRate:F1}%");
Console.WriteLine($"Total validations: {summary.TotalValidations}");
```

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `MaxInputLength` | 10000 | Maximum input characters |
| `MaxOutputLength` | 50000 | Maximum output characters |
| `EnablePromptInjectionDetection` | true | Detect injection attacks |
| `EnablePiiDetection` | true | Detect personal information |
| `EnableToxicityFiltering` | true | Block harmful content |
| `EnableTopicRestriction` | false | Restrict to allowed topics |
| `AllowedTopics` | [] | Topics for restriction |
| `TopicSimilarityThreshold` | 0.5 | Semantic similarity threshold |
| `EnableRateLimiting` | false | Enable rate limiting |
| `RateLimitMaxRequests` | 60 | Max requests per window |
| `RateLimitWindowSeconds` | 60 | Rate limit window duration |
| `EnableLlmModeration` | false | Use LLM for moderation |
| `BlockedKeywords` | [] | Custom blocked words |
| `BlockedPatterns` | [] | Custom regex patterns |
| `DefaultAction` | Block | Action: Block, LogOnly, or Redact |

### OpenTelemetry Integration

Guardrails includes built-in OpenTelemetry instrumentation for distributed tracing and metrics.

```csharp
// Metrics and tracing are enabled by default
var service = new GuardrailsService(options, metrics, enableTelemetry: true);

// Or disable if needed
var service = new GuardrailsService(enableTelemetry: false);
```

**Metrics exported:**
| Metric | Type | Description |
|--------|------|-------------|
| `guardrails.validations.total` | Counter | Total validations performed |
| `guardrails.validations.blocked` | Counter | Blocked validations |
| `guardrails.validations.allowed` | Counter | Allowed validations |
| `guardrails.violations` | Counter | Violations by type |
| `guardrails.rate_limit.hits` | Counter | Rate limit violations |
| `guardrails.validation.duration` | Histogram | Validation duration (ms) |

**Tracing:**
- Activity source: `AI.Workshop.Guardrails`
- Activities: `guardrails.validate_input`, `guardrails.validate_output`, `guardrails.validator.{name}`

**Aspire integration (automatic):**
The ServiceDefaults already includes guardrails instrumentation. Metrics and traces appear in your configured OTLP exporter (e.g., Aspire Dashboard, Jaeger, Prometheus).

---

## 📁 Prompty Templates

All system prompts are managed using [Prompty](https://prompty.ai/) `.prompty` files located in each project's `Prompts/` folder:

| Project | Prompts |
|---------|---------|
| Console.VectorDemos | `BookRecommendation`, `ServiceSuggestion`, `DocumentSearch` |
| Console.AgentChat | `GeneralAssistant`, `DocumentSearch`, `DocumentSearchSimple`, `PDFSummarization` |
| Console.Agents | `AgentSmith`, `WeatherAssistant`, `PersonInfo`, `SpanishTranslator`, `StoryWriter`, `StoryEditor` |
| WebApi.Agents | `StoryWriter`, `StoryEditor` |
| ChatApp.Web | `DocumentSearch`, `DocumentSearchSimple`, `GeneralAssistant`, `PDFSummarization` |
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

## 🗄️ Embedding Caching

The `AI.Workshop.Common.Caching` namespace provides caching wrappers for embedding generators to reduce API calls and improve performance.

### In-Memory Caching

```csharp
using AI.Workshop.Common.Caching;

// Option 1: Wrap existing generator
var cachedGenerator = embeddingGenerator.WithCaching(options =>
{
    options.MaxCacheSize = 10000;           // Max cached embeddings
    options.SlidingExpirationMinutes = 60;  // Cache duration
    options.CleanupIntervalMinutes = 5;     // Cleanup frequency
});

// Option 2: DI registration with caching
builder.Services.AddOllamaEmbeddingGeneratorWithCaching(configuration, options =>
{
    options.MaxCacheSize = 5000;
    options.SlidingExpirationMinutes = 120;
});

// Access cache statistics
var stats = cachedGenerator.GetStats();
Console.WriteLine($"Hit rate: {stats.HitRate:F1}%");
Console.WriteLine($"Cache size: {stats.CacheSize}");
```

### Distributed Caching (Redis, SQL Server)

```csharp
using AI.Workshop.Common.Caching;

// For distributed scenarios
var distributedGenerator = new DistributedCachedEmbeddingGenerator(
    innerGenerator,
    distributedCache,  // IDistributedCache (Redis, SQL, etc.)
    new DistributedEmbeddingCacheOptions
    {
        KeyPrefix = "emb:",
        SlidingExpiration = TimeSpan.FromHours(1)
    });
```

### Cache Statistics

| Property | Description |
|----------|-------------|
| `CacheHits` | Number of cache hits |
| `CacheMisses` | Number of cache misses |
| `CacheSize` | Current number of cached embeddings |
| `HitRate` | Cache hit rate as percentage |

---

## 📝 TOON (Token-Oriented Object Notation)

TOON is a compact data format that reduces LLM token usage by 30-60% compared to JSON. The `AI.Workshop.Common.Toon` namespace provides utilities for working with TOON.

### Basic Usage

```csharp
using AI.Workshop.Common.Toon;

// Serialize to TOON
var data = new { id = 1, name = "Alice", roles = new[] { "admin", "user" } };
var toon = ToonHelper.ToToon(data);
// Output:
// id: 1
// name: Alice
// roles[2]: admin,user

// Deserialize from TOON
var user = ToonHelper.FromToon<User>(toon);

// Convert between JSON and TOON
var toonFromJson = ToonHelper.JsonToToon(jsonString);
var jsonFromToon = ToonHelper.ToonToJson(toonString);

// Estimate token savings
var savings = ToonHelper.EstimateTokenSavings(data);
Console.WriteLine($"Savings: {savings.SavingsPercent:F1}%");
```

### Using TOON with Chat Clients

```csharp
using AI.Workshop.Common.Toon;

// Send data as TOON in prompt (reduces tokens)
var response = await chatClient.GetResponseWithToonDataAsync(
    data: products,
    userPrompt: "Which product is the most expensive?",
    systemPrompt: "You analyze product data.",
    dataLabel: "Products"
);

// Fluent prompt builder
var messages = ToonPromptBuilder.Create()
    .WithSystemPrompt("You are a data analyst.")
    .WithData(salesData, "Sales Data")
    .WithData(inventory, "Inventory")
    .UseCodeBlocks()
    .WithUserPrompt("Summarize the sales trends.")
    .Build();

var response = await chatClient.GetResponseAsync(messages);
```

### Parsing TOON Responses

```csharp
using AI.Workshop.Common.Toon;

// Request structured response with schema
var result = await chatClient.GetStructuredResponseAsync<ProductAnalysis>(
    prompt: "Analyze this product catalog",
    responseSchema: "bestSeller: string, averagePrice: number, totalProducts: number"
);

if (result.IsSuccess)
{
    Console.WriteLine($"Best seller: {result.Data.BestSeller}");
}

// Request with example (few-shot learning)
var example = new ProductAnalysis { BestSeller = "Widget", AveragePrice = 29.99, TotalProducts = 100 };
var result = await chatClient.GetStructuredResponseWithExampleAsync(
    prompt: "Analyze sales data",
    example: example,
    description: "Product analysis report"
);

// Parse TOON from any response
var parseResult = response.ParseToon<MyDataType>();
var data = parseResult.GetDataOrDefault(fallbackValue);

// Safe parsing
if (response.TryParseToon<MyDataType>(out var parsed))
{
    // Use parsed data
}
```

### TOON Format Benefits

| Aspect | JSON | TOON |
|--------|------|------|
| **Syntax** | Verbose (quotes, braces) | Minimal (indentation-based) |
| **Arrays** | Full key repetition | Tabular with header |
| **Token Usage** | Baseline | 30-60% less |
| **Readability** | Good | Excellent |

---

## 🎛️ Interactive Features

Both the Console Chat and Aspire Blazor Chat support interactive toggles for Guardrails and TOON, with token stats always displayed.

### Console Chat

Press `[S]` in the agent menu to access settings:
```
╔═══════════════════════════════════════════════════╗
║           AI Workshop - Settings Menu             ║
╠═══════════════════════════════════════════════════╣
║  [1] Guardrails: ENABLED - Content safety         ║
║  [2] TOON Format: DISABLED                        ║
║                                                   ║
║  📊 Token stats are always displayed              ║
║  [0] Back to main menu                            ║
╚═══════════════════════════════════════════════════╝
```

### Aspire Blazor Chat (ChatApp.Web)

Click the ⚙️ settings icon in the header to open the settings panel:
- **Agent Selection** – Choose between DocumentSearch and PDFSummarization agents
- Toggle **Guardrails** for content safety validation
- Toggle **TOON** for token-efficient data formatting
- **Token stats** are always displayed (no toggle needed)
- View **session token summary** with reset button

Active features are shown as badges: 📊 (Stats) 🛡️ (Guardrails) 📝 (TOON)

### Token Tracking (Always On)

Token usage is automatically tracked and displayed after each response:
- **Per-response**: `📊 Tokens: X in / Y out = Z total`
- **Session summary**: Total tokens, requests, and averages shown on exit (Console) or in settings panel (Blazor)
- **TOON stats**: When TOON is enabled, character savings are shown alongside results

### What Happens When Enabled

| Feature | Input | Output |
|---------|-------|--------|
| **Guardrails** | Blocks/redacts unsafe content | Validates AI responses |
| **TOON** | N/A | Formats search results compactly |
| **Token Stats** | N/A | Shows TOON vs XML comparison |

### Ollama Health Check

Console apps automatically check Ollama availability on startup:
```
Checking Ollama connection...
⣾ Connecting to http://localhost:11434/...
✓ Connected to Ollama (version 0.9.0)
```

If Ollama is not running, helpful instructions are shown:
```
⚠ Ollama is not available at http://localhost:11434/
Please ensure Ollama is running:
  1. Install from https://ollama.ai
  2. Run: ollama serve
  3. Or use Aspire: dotnet run --project Aspire/AI.Workshop.ChatApp.AppHost
```

---

## 🔧 Configuration

### AI Settings (appsettings.json)

AI settings can be configured via `appsettings.json` using the `AI` section:

```json
{
  "AI": {
    "OllamaUri": "http://localhost:11434/",
    "ChatModel": "llama3.2",
    "EmbeddingModel": "all-minilm",
    "VectorDimensions": 384,
    "QdrantHost": "localhost",
    "QdrantGrpcPort": 6334,
    "QdrantApiKey": ""
  }
}
```

**Dependency Injection (Recommended):**

```csharp
using AI.Workshop.Common;

// ASP.NET Core / Web API
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOllamaChatClient(builder.Configuration);
// IChatClient is now available via DI

// Console apps with Host
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOllamaServices(builder.Configuration);  // Both chat + embeddings
var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
var embedder = host.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
```

**Manual Configuration:**

```csharp
// Load settings manually
var aiSettings = builder.Configuration.GetAISettings();

// Or configure inline
builder.Services.AddOllamaServices(settings =>
{
    settings.OllamaUri = "http://my-ollama-server:11434/";
    settings.ChatModel = "mistral";
});
```

| Setting | Default | Description |
|---------|---------|-------------|
| `OllamaUri` | `http://localhost:11434/` | Ollama server endpoint |
| `ChatModel` | `llama3.2` | Model for chat completions |
| `EmbeddingModel` | `all-minilm` | Model for embeddings |
| `VectorDimensions` | `384` | Embedding vector dimensions |
| `QdrantHost` | `localhost` | Qdrant vector database host |
| `QdrantGrpcPort` | `6334` | Qdrant gRPC port (not HTTP 6333) |
| `QdrantApiKey` | `""` | Qdrant API key (empty = no auth) |

| DI Extension | Description |
|--------------|-------------|
| `AddOllamaChatClient()` | Registers `IChatClient` singleton |
| `AddOllamaEmbeddingGenerator()` | Registers `IEmbeddingGenerator` singleton |
| `AddOllamaServices()` | Registers both chat and embedding clients |

> **Note:** If the `AI` section is missing, defaults from `AIConstants` are used.

### Vector Store Selection

The VectorStore library provides two sets of ingestion classes for different backends:

| Namespace | Key Type | Distance Function | Use With |
|-----------|----------|-------------------|----------|
| `Ingestion` | `string` | CosineDistance | SQLite-Vec |
| `Ingestion.Qdrant` | `Guid` | DotProductSimilarity | Qdrant |

Set `VECTOR_STORE` environment variable to `Qdrant` or `Sqlite` in the Aspire ChatApp.

> **Note:** Separate namespaces exist because the `VectorStoreVectorAttribute` requires compile-time constants for distance function.

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
