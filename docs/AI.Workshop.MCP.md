# AI.Workshop.MCP

Model Context Protocol (MCP) implementations including server and client projects.

## Overview

```mermaid
graph TB
    subgraph "MCP Ecosystem"
        subgraph "Servers"
            CS[ConsoleServer<br/>stdio transport]
            HS[HttpServer<br/>HTTP transport]
        end
        
        subgraph "Clients"
            CC[ConsoleClient]
        end
        
        subgraph "External Servers"
            GH[GitHub MCP Server]
        end
    end
    
    CC --> CS
    CC --> GH
    CS --> OLL[Ollama]
    HS --> OLL
```

## MCP Architecture

```mermaid
graph LR
    subgraph "MCP Protocol"
        INIT[Initialize]
        TOOLS[Tools]
        RES[Resources]
        PROM[Prompts]
    end
    
    subgraph "Transport"
        STDIO[stdio]
        HTTP[HTTP/SSE]
    end
    
    subgraph "Lifecycle"
        L1[Client connects]
        L2[Capabilities exchange]
        L3[Tool/Resource discovery]
        L4[Request/Response]
    end
    
    INIT --> TOOLS & RES & PROM
    TOOLS --> STDIO & HTTP
    L1 --> L2 --> L3 --> L4
```

---

## AI.Workshop.MCP.ConsoleServer

MCP server with stdio transport, providing tools and resources.

### Architecture

```mermaid
graph TB
    subgraph "ConsoleServer"
        MAIN[Program.cs]
        SVR[McpServer]
        TRANS[StdioTransport]
    end
    
    subgraph "Tools"
        MT[MonkeyTool]
        WT[WeatherTool]
    end
    
    subgraph "Resources"
        CFG[ConfigResource]
        HELP[HelpResource]
    end
    
    subgraph "External"
        API[Monkey API]
    end
    
    MAIN --> SVR --> TRANS
    SVR --> MT & WT
    SVR --> CFG & HELP
    MT --> API
```

### Tool Definitions

```mermaid
classDiagram
    class MonkeyTool {
        +Name: get_random_monkey
        +Description: Gets a random monkey
        +InvokeAsync(): MonkeyInfo
    }
    
    class WeatherTool {
        +Name: get_weather
        +Description: Gets weather for a city
        +Parameters: city (string)
        +InvokeAsync(city): WeatherInfo
    }
    
    class MonkeyInfo {
        +Name: string
        +Species: string
        +Habitat: string
        +FunFact: string
    }
```

### Project Structure

```
MCP/AI.Workshop.MCP.ConsoleServer/
├── Program.cs              # Server entry point
├── Tools/
│   ├── MonkeyTool.cs
│   └── WeatherTool.cs
├── Resources/
│   ├── ConfigResource.cs
│   └── HelpResource.cs
└── appsettings.json
```

---

## AI.Workshop.MCP.ConsoleClient

MCP client that connects to local and external MCP servers.

### Architecture

```mermaid
graph TB
    subgraph "ConsoleClient"
        MAIN[Program.cs]
        CLI[McpClient]
        NAV[ServerNavigator]
    end
    
    subgraph "Connected Servers"
        LOCAL[Local ConsoleServer]
        GITHUB[GitHub MCP Server]
    end
    
    subgraph "AI Integration"
        CC[IChatClient]
        OLL[Ollama]
    end
    
    subgraph "Prompty Templates"
        P1[MonkeyAssistant.prompty]
        P2[GitHubAssistant.prompty]
    end
    
    MAIN --> NAV --> CLI
    CLI --> LOCAL & GITHUB
    NAV --> CC --> OLL
    NAV --> P1 & P2
```

### Client Flow

```mermaid
sequenceDiagram
    participant User
    participant Client
    participant LocalServer
    participant GitHubServer
    participant Ollama
    
    User->>Client: Select server
    Client->>LocalServer: Initialize
    LocalServer-->>Client: Capabilities + Tools
    
    User->>Client: "Tell me about monkeys"
    Client->>Ollama: Process with tools
    Ollama->>LocalServer: get_random_monkey()
    LocalServer-->>Ollama: MonkeyInfo
    Ollama-->>Client: Response with monkey facts
    Client-->>User: Display response
    
    User->>Client: Switch to GitHub
    Client->>GitHubServer: Initialize
    GitHubServer-->>Client: GitHub tools available
```

### Project Structure

```
MCP/AI.Workshop.MCP.ConsoleClient/
├── Program.cs              # Client entry point
├── ServerNavigator.cs      # Server selection UI
├── Prompts/
│   ├── MonkeyAssistant.prompty
│   └── GitHubAssistant.prompty
└── appsettings.json
```

---

## AI.Workshop.MCP.HttpServer

Minimal MCP server using HTTP transport with ASP.NET Core.

### Architecture

```mermaid
graph TB
    subgraph "HttpServer"
        APP[ASP.NET Core]
        MCP[MCP Middleware]
        SSE[Server-Sent Events]
    end
    
    subgraph "Endpoints"
        INIT[POST /mcp/initialize]
        TOOL[POST /mcp/tools/call]
        LIST[GET /mcp/tools/list]
    end
    
    subgraph "Tools"
        ECHO[EchoTool]
        TIME[TimeTool]
    end
    
    APP --> MCP --> SSE
    MCP --> INIT & TOOL & LIST
    TOOL --> ECHO & TIME
```

### HTTP Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/mcp/initialize` | POST | Initialize MCP session |
| `/mcp/tools/list` | GET | List available tools |
| `/mcp/tools/call` | POST | Invoke a tool |
| `/mcp/resources/list` | GET | List resources |
| `/mcp/resources/read` | POST | Read a resource |

### Project Structure

```
MCP/AI.Workshop.MCP.HttpServer/
├── Program.cs              # Minimal API entry
├── McpEndpoints.cs         # Endpoint definitions
├── Tools/
│   ├── EchoTool.cs
│   └── TimeTool.cs
└── appsettings.json
```

---

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| ModelContextProtocol | 0.4.1 | MCP SDK |
| ASP.NET Core | 10.0 | HTTP server (HttpServer only) |
| Ollama | - | LLM backend (Client) |
| Spectre.Console | - | Interactive UI (Client) |

## Usage

### Start Console Server

```bash
cd MCP/AI.Workshop.MCP.ConsoleServer
dotnet run
```

### Start HTTP Server

```bash
cd MCP/AI.Workshop.MCP.HttpServer
dotnet run
# Server available at http://localhost:5000
```

### Run Client

```bash
cd MCP/AI.Workshop.MCP.ConsoleClient
dotnet run
```

**Client Menu:**
```
╔═══════════════════════════════════════════════════╗
║            MCP Client - Server Selection          ║
╠═══════════════════════════════════════════════════╣
║  [1] Local Monkey Server (stdio)                  ║
║  [2] GitHub MCP Server (external)                 ║
║  [0] Exit                                         ║
╚═══════════════════════════════════════════════════╝
```

## MCP Configuration (.mcp.json)

```json
{
  "mcpServers": {
    "local-monkey": {
      "command": "dotnet",
      "args": ["run", "--project", "MCP/AI.Workshop.MCP.ConsoleServer"]
    },
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"]
    }
  }
}
```
