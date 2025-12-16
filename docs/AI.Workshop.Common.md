# AI.Workshop.Common

Shared library providing utilities, DI extensions, TOON serialization, embedding caching, and health checks.

## Architecture

```mermaid
graph TB
    subgraph "AI.Workshop.Common"
        subgraph "Configuration"
            AIS[AISettings]
            AIC[AIConstants]
            CFG[ConfigurationExtensions]
        end
        
        subgraph "Dependency Injection"
            DIE[ServiceCollectionExtensions]
            OCE[AddOllamaChatClient]
            OEE[AddOllamaEmbeddingGenerator]
            OAS[AddOllamaServices]
        end
        
        subgraph "TOON Serialization"
            TH[ToonHelper]
            TPB[ToonPromptBuilder]
            TSF[ToonSearchFormatter]
            TCE[ChatClientExtensions]
        end
        
        subgraph "Caching"
            CEG[CachedEmbeddingGenerator]
            DCE[DistributedCachedEmbeddingGenerator]
            ECO[EmbeddingCacheOptions]
            CS[CacheStats]
        end
        
        subgraph "Health Checks"
            OHC[OllamaHealthCheck]
        end
        
        subgraph "Utilities"
            PH[PromptyHelper]
            TUT[TokenUsageTracker]
            AM[AgentMetadata]
        end
    end
    
    subgraph "External Dependencies"
        OLL[Ollama API]
        MEAI[Microsoft.Extensions.AI]
        TS[ToonSharp]
        PRO[Prompty.Core]
    end
    
    DIE --> OLL
    TH --> TS
    PH --> PRO
    OCE --> MEAI
    OHC --> OLL
```

## Component Details

### Configuration

| Component | Purpose |
|-----------|---------|
| `AISettings` | POCO for AI configuration (models, endpoints) |
| `AIConstants` | Default values for Ollama URI, models, dimensions |
| `ConfigurationExtensions` | `GetAISettings()` extension for IConfiguration |

### Dependency Injection

| Extension | Registers |
|-----------|-----------|
| `AddOllamaChatClient()` | `IChatClient` singleton |
| `AddOllamaEmbeddingGenerator()` | `IEmbeddingGenerator<string, Embedding<float>>` singleton |
| `AddOllamaServices()` | Both chat and embedding clients |
| `AddOllamaEmbeddingGeneratorWithCaching()` | Embedding generator with in-memory cache |

### TOON (Token-Oriented Object Notation)

```mermaid
flowchart LR
    subgraph Input
        JSON[JSON Data]
        OBJ[C# Object]
    end
    
    subgraph ToonHelper
        TO[ToToon]
        FROM[FromToon]
        J2T[JsonToToon]
        T2J[ToonToJson]
        EST[EstimateTokenSavings]
    end
    
    subgraph Output
        TOON[TOON String]
        SAV[TokenSavingsInfo]
    end
    
    OBJ --> TO --> TOON
    TOON --> FROM --> OBJ
    JSON --> J2T --> TOON
    TOON --> T2J --> JSON
    OBJ --> EST --> SAV
```

**Token Savings Example:**
```
JSON (156 chars):
{"results":[{"filename":"doc.pdf","page":1,"text":"Content..."}]}

TOON (89 chars):
results[1]:
  filename|page|text
  doc.pdf|1|Content...

Savings: ~43%
```

### Embedding Caching

```mermaid
flowchart TB
    subgraph Application
        REQ[Embedding Request]
    end
    
    subgraph CachedEmbeddingGenerator
        CHK{Cache Hit?}
        GEN[Generate Embedding]
        STORE[Store in Cache]
        RET[Return Embedding]
    end
    
    subgraph Cache
        MEM[In-Memory Cache]
        DIST[Distributed Cache]
    end
    
    subgraph External
        OLL[Ollama all-minilm]
    end
    
    REQ --> CHK
    CHK -->|Yes| RET
    CHK -->|No| GEN
    GEN --> OLL
    OLL --> STORE
    STORE --> MEM
    STORE --> DIST
    STORE --> RET
```

### Health Checks

```mermaid
sequenceDiagram
    participant App
    participant OllamaHealthCheck
    participant Ollama
    
    App->>OllamaHealthCheck: CheckHealthAsync()
    OllamaHealthCheck->>Ollama: GET /api/version
    
    alt Ollama Running
        Ollama-->>OllamaHealthCheck: 200 OK + version
        OllamaHealthCheck-->>App: Healthy (version info)
    else Ollama Not Running
        Ollama-->>OllamaHealthCheck: Connection refused
        OllamaHealthCheck-->>App: Unhealthy (error details)
    end
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Microsoft.Extensions.AI | 10.0.1 | AI abstractions |
| ToonSharp | 1.0.0 | TOON serialization |
| Prompty.Core | 0.2.3 | Prompt templates |
| OllamaSharp | 5.4.11 | Ollama API client |

## Usage

```csharp
// Configuration via DI
builder.Services.AddOllamaServices(builder.Configuration);

// TOON serialization
var toon = ToonHelper.ToToon(myData);
var savings = ToonHelper.EstimateTokenSavings(myData);

// Cached embeddings
var generator = embeddingGenerator.WithCaching(opts => {
    opts.MaxCacheSize = 10000;
});

// Health check
builder.Services.AddHealthChecks()
    .AddCheck<OllamaHealthCheck>("ollama");
```
