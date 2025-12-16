# AI.Workshop.Console.Agents

Console application demonstrating Microsoft Agent Framework capabilities with various agent patterns.

## Architecture

```mermaid
graph TB
    subgraph "Console Application"
        MAIN[Program.cs]
        MENU[Demo Menu]
    end
    
    subgraph "Demos"
        D1[Weather Agent]
        D2[Person Info Agent]
        D3[Story Pipeline]
        D4[Translation Agent]
    end
    
    subgraph "Agent Framework"
        CCA[ChatClientAgent]
        RT[AgentRuntime]
        WF[Workflow Patterns]
    end
    
    subgraph "Tools"
        WT[WeatherTool]
        PT[PersonTool]
    end
    
    subgraph "AI Services"
        OLL[Ollama llama3.2]
    end
    
    subgraph "Prompty Templates"
        P1[WeatherAssistant.prompty]
        P2[PersonInfo.prompty]
        P3[StoryWriter.prompty]
        P4[StoryEditor.prompty]
        P5[CroatianTranslator.prompty]
    end
    
    MAIN --> MENU --> D1 & D2 & D3 & D4
    D1 & D2 & D3 & D4 --> CCA --> RT
    D1 --> WT
    D2 --> PT
    CCA --> OLL
    D1 --> P1
    D2 --> P2
    D3 --> P3 & P4
    D4 --> P5
```

## Agent Patterns

### 1. Tool-Calling Agent (Weather)

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant LLM
    participant WeatherTool
    
    User->>Agent: What's the weather in Zagreb?
    Agent->>LLM: Process with tools
    LLM->>WeatherTool: get_weather("Zagreb")
    WeatherTool-->>LLM: {"temp": 22, "conditions": "Sunny"}
    LLM-->>Agent: It's 22°C and sunny in Zagreb
    Agent-->>User: Response
```

### 2. Sequential Pipeline (Story Writer → Editor)

```mermaid
sequenceDiagram
    participant User
    participant Runtime
    participant Writer
    participant Editor
    participant LLM
    
    User->>Runtime: Topic: "Space exploration"
    Runtime->>Writer: Generate story
    Writer->>LLM: Write story prompt
    LLM-->>Writer: Draft story
    Writer->>Runtime: Pass to Editor
    Runtime->>Editor: Edit story
    Editor->>LLM: Edit prompt
    LLM-->>Editor: Polished story
    Editor-->>Runtime: Final story
    Runtime-->>User: Published story
```

### 3. Multi-Step Agent (Person Info)

```mermaid
graph LR
    subgraph "Tools"
        T1[GetPersonInfo]
        T2[GetContactDetails]
        T3[GetEmploymentHistory]
    end
    
    subgraph "Agent"
        Q[Query]
        TC[Tool Calls]
        AGG[Aggregate]
        RES[Response]
    end
    
    Q --> TC
    TC --> T1 & T2 & T3
    T1 & T2 & T3 --> AGG --> RES
```

### 4. Stateless Translation Agent

```mermaid
graph LR
    subgraph "Input"
        EN[English Text]
    end
    
    subgraph "Agent"
        P[CroatianTranslator.prompty]
        LLM[Ollama]
    end
    
    subgraph "Output"
        HR[Croatian Text]
    end
    
    EN --> P --> LLM --> HR
```

## Microsoft Agent Framework

```mermaid
classDiagram
    class ChatClientAgent {
        +Name: string
        +SystemPrompt: string
        +Tools: AIFunction[]
        +HandleAsync(message)
    }
    
    class AgentRuntime {
        +RegisterAgent(agent)
        +SendMessageAsync(agent, message)
    }
    
    class AIFunction {
        +Name: string
        +Description: string
        +Parameters: JsonSchema
        +InvokeAsync(args)
    }
    
    AgentRuntime --> ChatClientAgent
    ChatClientAgent --> AIFunction
```

## Demo Descriptions

### Weather Agent
- **Pattern:** Tool-calling
- **Tools:** `get_weather(city)`
- **Prompt:** WeatherAssistant.prompty
- **Demo:** Natural language weather queries

### Person Info Agent
- **Pattern:** Multi-tool orchestration
- **Tools:** `GetPersonInfo`, `GetContactDetails`, `GetEmploymentHistory`
- **Prompt:** PersonInfo.prompty
- **Demo:** Aggregate information from multiple sources

### Story Pipeline
- **Pattern:** Sequential workflow
- **Agents:** Writer → Editor
- **Prompts:** StoryWriter.prompty, StoryEditor.prompty
- **Demo:** Two-agent content creation pipeline

### Translation Agent
- **Pattern:** Stateless transformation
- **Tools:** None
- **Prompt:** CroatianTranslator.prompty
- **Demo:** Direct text translation

## Project Structure

```
AI.Workshop.Console.Agents/
├── Program.cs              # Entry point with menu
├── Demos/
│   ├── WeatherAgentDemo.cs
│   ├── PersonInfoDemo.cs
│   ├── StoryPipelineDemo.cs
│   └── TranslationDemo.cs
├── Tools/
│   ├── WeatherTool.cs
│   └── PersonTool.cs
├── Prompts/
│   ├── WeatherAssistant.prompty
│   ├── PersonInfo.prompty
│   ├── StoryWriter.prompty
│   ├── StoryEditor.prompty
│   └── CroatianTranslator.prompty
└── AgentSmith.prompty      # General agent template
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Microsoft.Agents.AI | 0.8.2 | Agent Framework |
| Ollama | - | LLM backend |
| Prompty.Core | 0.2.3 | Prompt templates |
| Spectre.Console | - | Interactive UI |

## Usage

```bash
cd AI.Workshop.Console.Agents
dotnet run
```

**Menu Options:**
```
╔═══════════════════════════════════════════════════╗
║        AI.Workshop - Agent Framework Demos        ║
╠═══════════════════════════════════════════════════╣
║  [1] Weather Agent (Tool Calling)                 ║
║  [2] Person Info Agent (Multi-Tool)               ║
║  [3] Story Pipeline (Writer → Editor)             ║
║  [4] Translation Agent (Croatian)                 ║
║  [0] Exit                                         ║
╚═══════════════════════════════════════════════════╝
```
