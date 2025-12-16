# AI.Workshop.VectorStore

Vector database integrations and RAG (Retrieval-Augmented Generation) pipelines for PDF and GitHub ingestion.

## Architecture

```mermaid
graph TB
    subgraph "Data Sources"
        PDF[PDF Documents]
        GH[GitHub Repos]
    end
    
    subgraph "Ingestion Pipeline"
        subgraph "PDF Processing"
            PDFR[PdfReader]
            CHK[TextChunker]
        end
        
        subgraph "GitHub Processing"
            GHI[GitHubIngestion]
            FILEF[File Filters]
        end
        
        EMB[Embedding Generator]
    end
    
    subgraph "Vector Stores"
        subgraph "SQLite Backend"
            SQL[SQLite-Vec]
            SS[SemanticSearch]
        end
        
        subgraph "Qdrant Backend"
            QD[Qdrant]
            QSS[Qdrant.SemanticSearch]
        end
        
        subgraph "In-Memory"
            IMS[InMemoryVectorStore]
        end
    end
    
    subgraph "Search"
        SEM[Semantic Search]
        RES[SearchResult]
    end
    
    PDF --> PDFR --> CHK --> EMB
    GH --> GHI --> FILEF --> EMB
    EMB --> SQL & QD & IMS
    SQL --> SS --> SEM
    QD --> QSS --> SEM
    IMS --> SEM
    SEM --> RES
```

## Ingestion Flow

```mermaid
sequenceDiagram
    participant Source as Data Source
    participant Reader as PdfReader/GitHubIngestion
    participant Chunker as TextChunker
    participant Embedder as IEmbeddingGenerator
    participant Store as VectorStore
    
    Source->>Reader: Load Document
    Reader->>Reader: Extract Text
    Reader->>Chunker: Raw Text
    
    loop For Each Chunk
        Chunker->>Chunker: Split (512 chars, 100 overlap)
        Chunker->>Embedder: Chunk Text
        Embedder->>Embedder: Generate Embedding (384 dims)
        Embedder->>Store: Store Chunk + Embedding
    end
    
    Store-->>Source: Ingestion Complete
```

## Vector Store Implementations

### SQLite-Vec (Default)

```mermaid
graph LR
    subgraph "SQLite-Vec"
        DB[(SQLite Database)]
        VEC[Vec Extension]
        IDX[Vector Index]
    end
    
    subgraph "Schema"
        TBL[text_chunks]
        COL1[document_id: TEXT]
        COL2[page_number: INT]
        COL3[text: TEXT]
        COL4[embedding: BLOB]
    end
    
    DB --> VEC --> IDX
    TBL --> COL1 & COL2 & COL3 & COL4
```

**Key Type:** `string`  
**Distance Function:** Cosine Distance

### Qdrant

```mermaid
graph LR
    subgraph "Qdrant"
        SRV[Qdrant Server]
        COL[Collection]
        VEC[Vectors]
        PAY[Payloads]
    end
    
    subgraph "Configuration"
        DIM[Dimensions: 384]
        DIST[Distance: DotProduct]
        SHARD[Sharding]
    end
    
    SRV --> COL --> VEC & PAY
    COL --> DIM & DIST & SHARD
```

**Key Type:** `Guid`  
**Distance Function:** Dot Product Similarity

### In-Memory

```mermaid
graph TB
    subgraph "InMemoryVectorStore"
        DICT[Dictionary<string, Vector>]
        SEARCH[Linear Search]
    end
    
    subgraph "Use Cases"
        TEST[Unit Testing]
        DEMO[Demos]
        SMALL[Small Datasets]
    end
    
    DICT --> SEARCH
    SEARCH --> TEST & DEMO & SMALL
```

## Semantic Search

```mermaid
flowchart TB
    subgraph "Query Processing"
        Q[User Query]
        QE[Query Embedding]
    end
    
    subgraph "Vector Search"
        VS[Vector Store]
        SIM[Similarity Calculation]
        RANK[Ranking]
    end
    
    subgraph "Results"
        TOP[Top K Results]
        CHK[Text Chunks]
        META[Metadata]
    end
    
    Q --> QE --> VS
    VS --> SIM --> RANK --> TOP
    TOP --> CHK & META
```

### Search Options

| Parameter | Default | Description |
|-----------|---------|-------------|
| `searchPhrase` | required | Query text to embed and search |
| `filenameFilter` | null | Optional document filter |
| `topK` | 5 | Number of results to return |

## Data Models

```mermaid
classDiagram
    class TextChunk {
        +string DocumentId
        +int PageNumber
        +string Text
        +ReadOnlyMemory~float~ Embedding
    }
    
    class QdrantTextChunk {
        +Guid Key
        +string DocumentId
        +int PageNumber
        +string Text
        +ReadOnlyMemory~float~ Embedding
    }
    
    class SearchResult {
        +string DocumentId
        +int PageNumber
        +string Text
        +float Score
    }
    
    TextChunk <|-- QdrantTextChunk
```

## Chunking Strategy

```mermaid
graph LR
    subgraph "Document"
        D1[Page 1 Text...]
        D2[Page 2 Text...]
        D3[Page 3 Text...]
    end
    
    subgraph "Chunking (512 chars, 100 overlap)"
        C1[Chunk 1]
        C2[Chunk 2]
        C3[Chunk 3]
        C4[Chunk 4]
    end
    
    subgraph "Overlap"
        O1[Last 100 chars of C1]
        O2[First 100 chars of C2]
    end
    
    D1 & D2 & D3 --> C1 & C2 & C3 & C4
    O1 -.->|Overlap| O2
```

**Configuration:**
- Chunk Size: 512 characters
- Overlap: 100 characters
- Embedding Model: all-minilm (384 dimensions)

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Microsoft.SemanticKernel.Connectors.Sqlite | 1.67.1 | SQLite vector store |
| Microsoft.SemanticKernel.Connectors.Qdrant | 1.67.1 | Qdrant vector store |
| Microsoft.SemanticKernel.Connectors.InMemory | 1.67.1 | In-memory store |
| PdfPig | 0.1.9 | PDF text extraction |

## Usage

```csharp
// SQLite ingestion
var ingestion = new PdfIngestion(embeddingGenerator);
await ingestion.IngestAsync(vectorStore, "documents/");

// Semantic search
var search = new SemanticSearch(vectorStore, embeddingGenerator);
var results = await search.SearchAsync("climate change impacts", topK: 5);

// With filename filter
var filtered = await search.SearchAsync(
    "product specifications", 
    filenameFilter: "manual.pdf", 
    topK: 3);

// Qdrant (different namespace)
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;
var qdrantSearch = new QdrantBased.SemanticSearch(qdrantStore, embeddingGenerator);
```
