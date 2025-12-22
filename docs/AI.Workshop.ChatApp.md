# AI.Workshop.ChatApp (Aspire)

Full-featured Blazor Server chat application orchestrated by .NET Aspire with Ollama and Qdrant containers.

## Architecture Overview

```mermaid
graph TB
    subgraph "Aspire AppHost"
        ORCH[Orchestrator]
    end
    
    subgraph "Containers"
        OLL[Ollama Container]
        QD[Qdrant Container]
    end
    
    subgraph "ChatApp.Web"
        BLZR[Blazor Server]
        CHAT[Chat UI]
        SET[Settings Panel]
        STATS[Stats Bar]
    end
    
    subgraph "Services"
        CSS[ChatSettingsService]
        RS[RagService]
        GS[GuardrailsService]
    end
    
    subgraph "AI Pipeline"
        CC[IChatClient]
        GCC[GuardrailsChatClient]
        FI[FunctionInvocation]
    end
    
    subgraph "Vector Store"
        VS[Qdrant/SQLite]
        SS[SemanticSearch]
        ING[PdfIngestion]
    end
    
    ORCH --> OLL & QD
    ORCH --> BLZR
    BLZR --> CHAT & SET & STATS
    CHAT --> CSS & RS & GS
    RS --> SS --> VS
    CHAT --> CC --> GCC --> FI --> OLL
    ING --> VS
```

## Aspire Orchestration

```mermaid
graph LR
    subgraph "AppHost"
        MAIN[Program.cs]
        GPU[GPU Detection]
    end
    
    subgraph "Resources"
        OLL[Ollama<br/>llama3.2, all-minilm]
        QD[Qdrant<br/>Vector Database]
        WEB[ChatApp.Web<br/>Blazor Server]
    end
    
    subgraph "Configuration"
        ENV[Environment Variables]
        CONN[Connection Strings]
        VOL[Data Volumes]
    end
    
    MAIN --> GPU
    GPU --> OLL
    MAIN --> QD & WEB
    OLL & QD --> CONN --> WEB
    QD --> VOL
```

### GPU Detection

```mermaid
flowchart TB
    START[Start] --> CHECK{GPU_VENDOR set?}
    CHECK -->|Yes| USE[Use specified]
    CHECK -->|No| DETECT[Auto-detect]
    
    DETECT --> NV{NVIDIA?}
    NV -->|Yes| NVIDIA[Container Toolkit]
    NV -->|No| AMD{AMD?}
    AMD -->|Yes| AMDC[ROCm/CPU fallback]
    AMD -->|No| INTEL{Intel?}
    INTEL -->|Yes| INTC[Device passthrough]
    INTEL -->|No| CPU[CPU only]
    
    USE & NVIDIA & AMDC & INTC & CPU --> CONFIG[Configure Container]
```

## Blazor Components

```mermaid
graph TB
    subgraph "Layout"
        ML[MainLayout]
        HD[ChatHeader]
        SP[SettingsPanel]
    end
    
    subgraph "Chat"
        CHAT[Chat.razor]
        AS[AgentSelector]
        ML2[ChatMessageList]
        MI[ChatMessageItem]
        CI[ChatInput]
        CS[ChatSuggestions]
        SB[StatsBar]
    end
    
    subgraph "Alerts"
        GA[GuardrailAlert]
        CC[ChatCitation]
    end
    
    ML --> HD & SP
    CHAT --> AS & ML2 & CI & CS & SB
    ML2 --> MI
    CHAT --> GA & CC
```

## Chat Flow

```mermaid
sequenceDiagram
    participant User
    participant Chat
    participant Settings
    participant Guardrails
    participant RagService
    participant ChatClient
    participant Ollama
    
    User->>Chat: Select Agent
    Chat->>Settings: SetAgent(type)
    
    User->>Chat: Enter Message
    Chat->>Guardrails: ValidateInput()
    
    alt Input Blocked
        Guardrails-->>Chat: Violation
        Chat-->>User: Show Alert
    else Input Allowed
        Chat->>ChatClient: GetStreamingResponseAsync()
        
        loop Streaming
            ChatClient->>Ollama: Stream Request
            
            opt Tool Call (Search)
                Ollama->>RagService: SearchAsync()
                RagService-->>Ollama: TOON/XML Results
            end
            
            Ollama-->>ChatClient: Token Stream
            ChatClient-->>Chat: Update UI
        end
        
        Chat->>Guardrails: ValidateOutput()
        Chat->>Settings: UpdateStats()
        Chat-->>User: Show Response + Stats
    end
```

## Features

### Agent Selection

```mermaid
graph LR
    subgraph "Agents"
        DS[DocumentSearch<br/>Search & Answer]
        PS[PDFSummarization<br/>Summarize Documents]
        SG[StudyGuide<br/>Exhaustive Analysis]
    end
    
    subgraph "Prompty Files"
        P1[DocumentSearch.prompty]
        P2[PDFSummarization.prompty]
        P3[StudyGuide.prompty]
    end
    
    subgraph "Tools"
        T1[SearchDocuments]
        T2[ListDocuments]
        T3[GetDocumentContent]
    end
    
    DS --> P1 --> T1
    PS --> P2 --> T1 & T2
    SG --> P3 --> T1 & T2 & T3
```

**Agent Comparison:**

| Agent | Purpose | Tools | Best For |
|-------|---------|-------|----------|
| **DocumentSearch** | Quick lookups with citations | SearchDocuments | Specific questions, fact-checking |
| **PDFSummarization** | Chapter-by-chapter summaries | SearchDocuments, ListDocuments | Quick overviews, topic exploration |
| **StudyGuide** | Exhaustive study guides | All tools + GetDocumentContent | Exam prep, comprehensive learning |

### Stats Bar

```mermaid
graph LR
    subgraph "Response Stats"
        TIME[⏱️ Response Time]
        TKN[📊 Token Usage]
        FMT[📝 Format Comparison]
    end
    
    subgraph "TOON Comparison"
        TOON[TOON: X chars]
        XML[XML: Y chars]
        SAV[Savings: Z%]
    end
    
    TIME & TKN & FMT --> DISPLAY[Stats Bar]
    FMT --> TOON & XML --> SAV
```

### Theme Support

```mermaid
graph LR
    subgraph "Themes"
        DARK[Dark Theme<br/>Default]
        LIGHT[Light Theme]
    end
    
    subgraph "Toggle"
        BTN[🌙/☀️ Button]
        PREF[User Preference]
    end
    
    BTN --> PREF --> DARK & LIGHT
```

## Project Structure

```
Aspire/
├── AI.Workshop.ChatApp.AppHost/
│   ├── Program.cs              # Aspire orchestrator
│   └── GpuDetection.cs         # GPU auto-detection
│
├── AI.Workshop.ChatApp.Web/
│   ├── Program.cs              # Web app entry
│   ├── Components/
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   └── Pages/
│   │       └── Chat/
│   │           ├── Chat.razor
│   │           ├── ChatHeader.razor
│   │           ├── AgentSelector.razor
│   │           ├── ChatMessageList.razor
│   │           ├── ChatInput.razor
│   │           ├── StatsBar.razor
│   │           ├── SettingsPanel.razor
│   │           └── GuardrailAlert.razor
│   ├── Services/
│   │   ├── ChatSettingsService.cs
│   │   ├── RagService.cs
│   │   └── GuardrailsService.cs
│   ├── Prompts/
│   │   ├── DocumentSearch.prompty
│   │   ├── DocumentSearchSimple.prompty
│   │   ├── GeneralAssistant.prompty
│   │   ├── PDFSummarization.prompty
│   │   └── StudyGuide.prompty
│   └── wwwroot/
│       └── Data/               # PDF documents
│
└── AI.Workshop.ChatApp.ServiceDefaults/
    └── Extensions.cs           # OpenTelemetry, health checks
```

## Vector Store Selection

```mermaid
graph TB
    subgraph "Environment Variable"
        VS[VECTOR_STORE]
    end
    
    subgraph "Options"
        QD[Qdrant<br/>Distributed, persistent]
        SQL[SQLite-Vec<br/>Embedded, local]
    end
    
    VS -->|"Qdrant"| QD
    VS -->|"Sqlite"| SQL
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Aspire | 13.1 | Orchestration |
| Blazor Server | 10.0 | Web UI |
| Ollama | - | LLM + Embeddings |
| Qdrant | - | Vector database |
| SQLite-Vec | - | Embedded vector store |
| OpenTelemetry | - | Observability |

## Usage

```bash
cd Aspire/AI.Workshop.ChatApp.AppHost
dotnet run
```

**Aspire Dashboard:** `https://localhost:17000`

**Chat App:** `https://localhost:5001`

## Environment Variables

| Variable | Values | Description |
|----------|--------|-------------|
| `VECTOR_STORE` | `Qdrant`, `Sqlite` | Vector store backend |
| `GPU_VENDOR` | `nvidia`, `amd`, `intel`, `cpu` | GPU configuration |

## OpenTelemetry

```mermaid
graph LR
    subgraph "Instrumentation"
        APP[Application]
        GR[Guardrails]
        AI[AI Services]
    end
    
    subgraph "Exporters"
        OTLP[OTLP Exporter]
    end
    
    subgraph "Visualization"
        ASP[Aspire Dashboard]
        JAE[Jaeger]
        PROM[Prometheus]
    end
    
    APP & GR & AI --> OTLP --> ASP & JAE & PROM
```
