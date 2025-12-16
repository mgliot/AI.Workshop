# AI.Workshop.Console.AgentChat

Interactive console application with agent navigation, RAG workflows, guardrails, and TOON support.

## Architecture

```mermaid
graph TB
    subgraph "Console Application"
        MAIN[Program.cs]
        NAV[AgentNavigator]
        SET[ChatSettings]
    end
    
    subgraph "Agents"
        DS[DocumentSearch Agent]
        PS[PDFSummarization Agent]
    end
    
    subgraph "AI Pipeline"
        CC[IChatClient]
        GR[GuardrailsChatClient]
        FI[FunctionInvocation]
    end
    
    subgraph "RAG"
        VS[SQLite VectorStore]
        SS[SemanticSearch]
        PDF[PdfIngestion]
    end
    
    subgraph "Features"
        TOON[TOON Formatter]
        TKN[TokenUsageTracker]
        HC[OllamaHealthCheck]
    end
    
    subgraph "External"
        OLL[Ollama]
    end
    
    MAIN --> HC --> NAV
    NAV --> SET
    NAV --> DS & PS
    DS & PS --> CC
    CC --> GR --> FI --> OLL
    DS --> SS --> VS
    PDF --> VS
    DS --> TOON
    NAV --> TKN
```

## Agent Flow

```mermaid
sequenceDiagram
    participant User
    participant Navigator
    participant Agent
    participant Guardrails
    participant SearchTool
    participant VectorStore
    participant LLM
    
    User->>Navigator: Select Agent
    Navigator->>Agent: Initialize with Prompty
    
    loop Chat Session
        User->>Navigator: Enter Question
        Navigator->>Guardrails: Validate Input
        
        alt Input Blocked
            Guardrails-->>Navigator: Violation
            Navigator-->>User: Show Alert
        else Input Allowed
            Navigator->>Agent: Send Message
            Agent->>LLM: Stream Request
            
            opt Tool Call (Search)
                LLM->>SearchTool: search(phrase)
                SearchTool->>VectorStore: Semantic Search
                VectorStore-->>SearchTool: Results
                SearchTool-->>LLM: TOON/XML Results
            end
            
            LLM-->>Agent: Stream Response
            Agent->>Guardrails: Validate Output
            Agent-->>Navigator: Display Response + Tokens
        end
    end
```

## Agents

### DocumentSearch Agent

```mermaid
graph LR
    subgraph "Tools"
        SRCH[SearchAsync]
        LIST[ListDocumentsAsync]
    end
    
    subgraph "Process"
        Q[Question]
        EMB[Embed Query]
        RET[Retrieve Chunks]
        GEN[Generate Answer]
    end
    
    subgraph "Output"
        ANS[Contextual Answer]
        CIT[Citations]
    end
    
    Q --> EMB --> SRCH --> RET --> GEN --> ANS & CIT
```

### PDFSummarization Agent

```mermaid
graph LR
    subgraph "Tools"
        SRCH[SearchAsync]
        LIST[ListDocumentsAsync]
    end
    
    subgraph "Process"
        DOC[Select Document]
        EXT[Extract All Chunks]
        SUM[Summarize Content]
    end
    
    subgraph "Output"
        SUMM[Document Summary]
        KEY[Key Points]
    end
    
    DOC --> LIST --> EXT --> SRCH --> SUM --> SUMM & KEY
```

## Settings Menu

```mermaid
graph TB
    subgraph "Settings"
        GR[Guardrails Toggle]
        TOON[TOON Toggle]
        TKN[Token Stats]
    end
    
    subgraph "Guardrails Features"
        PI[Prompt Injection Detection]
        PII[PII Detection]
        TOX[Toxicity Filtering]
    end
    
    subgraph "TOON Features"
        FMT[Compact Formatting]
        SAV[Token Savings Display]
    end
    
    GR --> PI & PII & TOX
    TOON --> FMT & SAV
```

**Menu Display:**
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

## Project Structure

```
AI.Workshop.Console.AgentChat/
├── Program.cs              # Entry point
├── AgentNavigator.cs       # Main navigation and chat loop
├── ChatSettings.cs         # Runtime settings
├── Prompts/
│   ├── DocumentSearch.prompty
│   ├── DocumentSearchSimple.prompty
│   ├── GeneralAssistant.prompty
│   └── PDFSummarization.prompty
└── Data/
    └── *.pdf               # Documents for RAG
```

## TOON Integration

```mermaid
graph TB
    subgraph "Search Results"
        XML[XML Format]
        TOON[TOON Format]
    end
    
    subgraph "Comparison"
        CMP[FormatComparison]
        SAV[Savings %]
    end
    
    subgraph "Display"
        RES[Results to LLM]
        STAT[Stats to User]
    end
    
    XML --> CMP
    TOON --> CMP
    CMP --> SAV
    TOON --> RES
    SAV --> STAT
```

**Example Output:**
```
TOON (89 chars) vs XML (156 chars) - Savings: 43%
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Ollama | - | LLM + Embeddings |
| AI.Workshop.Common | - | TOON, caching, health checks |
| AI.Workshop.Guardrails | - | Content safety |
| AI.Workshop.VectorStore | - | RAG pipeline |
| Spectre.Console | - | Interactive UI |

## Usage

```bash
cd AI.Workshop.Console.AgentChat
dotnet run
```

**Commands:**
- Select agent from menu
- Type questions to chat
- Press `[S]` for settings
- Press `[B]` to switch agents
- Press `[Q]` to quit (shows session summary)
