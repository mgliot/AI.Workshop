# AI.Workshop.Console.VectorDemos

Console application demonstrating vector store capabilities with SQLite-Vec, Qdrant, and in-memory stores.

## Architecture

```mermaid
graph TB
    subgraph "Console Application"
        MAIN[Program.cs]
        MENU[Demo Menu]
    end
    
    subgraph "Demos"
        D1[Book Recommendation]
        D2[Service Suggestion]
        D3[Document Search RAG]
    end
    
    subgraph "Vector Stores"
        IMS[InMemoryVectorStore]
        SQL[SQLite-Vec]
        QD[Qdrant]
    end
    
    subgraph "AI Services"
        OLL[Ollama llama3.2]
        EMB[Ollama all-minilm]
    end
    
    subgraph "Prompty Templates"
        P1[BookRecommendation.prompty]
        P2[ServiceSuggestion.prompty]
        P3[DocumentSearch.prompty]
    end
    
    MAIN --> MENU --> D1 & D2 & D3
    D1 --> IMS --> EMB
    D2 --> IMS --> EMB
    D3 --> SQL --> EMB
    D1 & D2 & D3 --> OLL
    D1 --> P1
    D2 --> P2
    D3 --> P3
```

## Demo Flow

```mermaid
sequenceDiagram
    participant User
    participant Menu
    participant Demo
    participant VectorStore
    participant Ollama
    
    User->>Menu: Select Demo
    Menu->>Demo: Initialize
    
    alt Book/Service Demo
        Demo->>VectorStore: Populate Sample Data
        Demo->>Ollama: Generate Embeddings
        Ollama-->>VectorStore: Store Vectors
    else Document Search
        Demo->>VectorStore: Load PDF Chunks
    end
    
    User->>Demo: Enter Query
    Demo->>Ollama: Embed Query
    Ollama-->>Demo: Query Vector
    Demo->>VectorStore: Similarity Search
    VectorStore-->>Demo: Top Results
    Demo->>Ollama: Generate Response
    Ollama-->>Demo: LLM Response
    Demo-->>User: Display Answer
```

## Demo Descriptions

### 1. Book Recommendation

```mermaid
graph LR
    subgraph "Sample Data"
        B1[The Martian]
        B2[Dune]
        B3[1984]
        B4[Pride and Prejudice]
    end
    
    subgraph "Process"
        Q[User Query]
        E[Embed Query]
        S[Similarity Search]
        R[Recommend Books]
    end
    
    B1 & B2 & B3 & B4 --> S
    Q --> E --> S --> R
```

### 2. Service Suggestion

```mermaid
graph LR
    subgraph "Services"
        S1[Cloud Hosting]
        S2[Data Analytics]
        S3[AI/ML Platform]
        S4[Security Audit]
    end
    
    subgraph "Matching"
        REQ[Business Need]
        EMB[Embedding]
        MATCH[Best Match]
    end
    
    S1 & S2 & S3 & S4 --> MATCH
    REQ --> EMB --> MATCH
```

### 3. Document Search (RAG)

```mermaid
graph TB
    subgraph "Ingestion"
        PDF[PDF Documents]
        CHK[Text Chunks]
        VEC[Vector Index]
    end
    
    subgraph "Query"
        Q[User Question]
        CTX[Retrieved Context]
        ANS[AI Answer]
    end
    
    PDF --> CHK --> VEC
    Q --> VEC --> CTX --> ANS
```

## Project Structure

```
AI.Workshop.Console.VectorDemos/
├── Program.cs              # Entry point with menu
├── Demos/
│   ├── BookRecommendationDemo.cs
│   ├── ServiceSuggestionDemo.cs
│   └── DocumentSearchDemo.cs
├── Prompts/
│   ├── BookRecommendation.prompty
│   ├── ServiceSuggestion.prompty
│   └── DocumentSearch.prompty
└── Data/
    └── *.pdf               # Sample PDF documents
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Ollama | - | LLM (llama3.2) + Embeddings (all-minilm) |
| Microsoft.SemanticKernel.Connectors.InMemory | 1.67.1 | In-memory vector store |
| Microsoft.SemanticKernel.Connectors.Sqlite | 1.67.1 | SQLite vector store |
| Spectre.Console | - | Interactive console UI |

## Usage

```bash
cd AI.Workshop.Console.VectorDemos
dotnet run
```

**Menu Options:**
```
╔═══════════════════════════════════════════════════╗
║          AI.Workshop - Vector Demos               ║
╠═══════════════════════════════════════════════════╣
║  [1] Book Recommendation (In-Memory)              ║
║  [2] Service Suggestion (In-Memory)               ║
║  [3] Document Search (SQLite-Vec RAG)             ║
║  [0] Exit                                         ║
╚═══════════════════════════════════════════════════╝
```
