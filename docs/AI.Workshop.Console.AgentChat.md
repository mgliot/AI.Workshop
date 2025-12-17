# AI.Workshop.Console.AgentChat

Interactive console application demonstrating **AI agent capabilities** in a learning progression - from basic chat to full RAG. Includes guardrails, TOON support, and token tracking.

## Purpose

This project teaches AI agent patterns step-by-step:
1. **Basic Chat** - LLM interaction with conversation history
2. **Tool Calling** - Function invocation with CurrentTime tool
3. **Multi-Tool Demo** - Stateful shopping cart with multiple tools
4. **Full RAG** - Document search with vector store integration
5. **Prompt Engineering** - Compare different system prompts
6. **Advanced RAG** - PDF summarization with document listing

> **Note:** For vector store backend comparison, see `AI.Workshop.Console.VectorDemos`.

## Architecture

```mermaid
graph TB
    subgraph "Console Application"
        MAIN[Program.cs]
        NAV[AgentNavigator]
        SET[ChatSettings]
    end
    
    subgraph "Learning Progression"
        S1[Step 1: Basic Chat]
        S2[Step 2: Chat + Tools]
        S3[Step 3: Multi-Tool Demo]
    end
    
    subgraph "Full RAG Demos"
        DS[DocumentSearch Agent]
        DSS[DocumentSearch Simple]
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
    NAV --> S1 & S2 & S3
    NAV --> DS & DSS & PS
    S1 --> CC --> OLL
    S2 --> CC --> FI --> OLL
    S3 --> CC --> FI --> OLL
    DS & DSS & PS --> CC
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

## Demo Descriptions

### Step 1: Basic Chat
- Pure LLM interaction with streaming responses
- Conversation history for context retention
- System prompt defines assistant behavior
- **No tools** - foundation for understanding chat mechanics

### Step 2: Chat + Tools
- Introduces `AIFunctionFactory` for tool creation
- LLM decides when to call `CurrentTime` tool
- `UseFunctionInvocation()` middleware
- Tool results integrated into responses

### Step 3: Multi-Tool Demo (Shopping Cart)
- Multiple tools working together (pricing, cart management)
- Stateful tool (Cart class maintains state)
- LLM orchestrates tool calls
- Real-world function calling pattern

---

## Full RAG Agents

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
╔════════════════════════════════════════════════════════════════════════╗
║              AI Workshop - Agent Chat Demos                            ║
║                                                                        ║
║  Learn AI agent capabilities from basic chat to full RAG               ║
╠════════════════════════════════════════════════════════════════════════╣
║  🛡️ Guardrails: ON | 📝 TOON: OFF                                      ║
╠════════════════════════════════════════════════════════════════════════╣
║  LEARNING PROGRESSION:                                                 ║
╠════════════════════════════════════════════════════════════════════════╣
║  [1] Step 1: Basic Chat          Simple chat loop with history         ║
║  [2] Step 2: Chat + Tools        Adds CurrentTime tool                 ║
║  [3] Step 3: Multi-Tool Demo     Shopping cart with multiple tools     ║
╠════════════════════════════════════════════════════════════════════════╣
║  FULL RAG IMPLEMENTATIONS:                                             ║
╠════════════════════════════════════════════════════════════════════════╣
║  [4] Document Search             Complete RAG with citations           ║
║  [5] Document Search (Simple)    Simplified prompt variant             ║
║  [6] PDF Summarization           Document summarization                ║
╠════════════════════════════════════════════════════════════════════════╣
║  [S] Settings - Toggle Guardrails, TOON                                ║
║  [0] Exit                                                              ║
╚════════════════════════════════════════════════════════════════════════╝
```

**Settings Menu:**
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
├── Program.cs              # Entry point with health check
├── AgentNavigator.cs       # Main navigation, demo selection, chat loop
├── ChatSettings.cs         # Runtime settings (Guardrails, TOON)
├── BasicToolsExamples.cs   # Step 3: Shopping cart demo
├── RagWorkflowExamples.cs  # Legacy RAG examples (reference)
├── InMemoryVectorStoreSearch.cs  # Legacy vector search (reference)
├── Tools/
│   └── CurrentTimeTool.cs  # Example tool for Step 2
├── Prompts/
│   ├── GeneralAssistant.prompty   # Steps 1-3
│   ├── DocumentSearch.prompty     # Full RAG
│   ├── DocumentSearchSimple.prompty
│   └── PDFSummarization.prompty
└── Data/
    └── *.pdf               # Documents for RAG demos
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
