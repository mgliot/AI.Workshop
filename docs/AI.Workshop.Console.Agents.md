# AI.Workshop.Console.Agents

Console application demonstrating Microsoft Agent Framework capabilities with various agent patterns.

## Architecture

```mermaid
graph TB
    subgraph "Console Application"
        MAIN[Program.cs]
        DEMOS[Sequential Demos]
    end
    
    subgraph "Demos"
        D1[GhostWriter Workflow]
        D2[Agent Smith Prompt]
        D3[Agent Smith Conversation]
        D4[Weather Function Calling]
        D5[Structured Output]
        D6[Agent-as-Tool]
    end
    
    subgraph "Agent Framework"
        CCA[ChatClientAgent]
        WF[Workflow Builder]
        AAT[Agent as AIFunction]
    end
    
    subgraph "Tools"
        WT[WeatherTools]
        FMT[FormatStory/GetAuthor]
    end
    
    subgraph "AI Services"
        OLL[Ollama llama3.2]
    end
    
    subgraph "Prompty Templates"
        P1[StoryWriter.prompty]
        P2[StoryEditor.prompty]
        P3[AgentSmith.prompty]
        P4[WeatherAssistant.prompty]
        P5[PersonInfo.prompty]
        P6[SpanishTranslator.prompty]
    end
    
    MAIN --> DEMOS --> D1 & D2 & D3 & D4 & D5 & D6
    D1 --> WF --> CCA
    D2 & D3 --> CCA
    D4 & D5 --> CCA
    D6 --> AAT --> CCA
    D1 --> FMT
    D4 & D6 --> WT
    CCA --> OLL
    D1 --> P1 & P2
    D2 & D3 --> P3
    D4 --> P4
    D5 --> P5
    D6 --> P4 & P6
```

## Agent Patterns

### 1. Sequential Workflow (GhostWriter)

```mermaid
sequenceDiagram
    participant User
    participant Workflow
    participant Writer
    participant Editor
    participant LLM
    
    User->>Workflow: "Write a story about a haunted house"
    Workflow->>Writer: Generate story
    Writer->>LLM: Write story prompt + tools
    LLM->>Writer: Call GetAuthor(), FormatStory()
    Writer-->>Workflow: Draft story
    Workflow->>Editor: Edit story
    Editor->>LLM: Edit prompt
    LLM-->>Editor: Polished story
    Editor-->>Workflow: Final story
    Workflow-->>User: Published story
```

### 2. Single Prompt Agent (Agent Smith)

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant LLM
    
    User->>Agent: "What is the matrix?"
    Agent->>LLM: Process with AgentSmith persona
    LLM-->>Agent: Philosophical response
    Agent-->>User: Response
```

### 3. Multi-turn Conversation (Agent Smith)

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant Thread
    participant LLM
    
    User->>Agent: "I know what you are, Smith."
    Agent->>Thread: Add to conversation history
    Agent->>LLM: Process with context
    LLM-->>Agent: Response 1
    Agent-->>User: Response
    
    User->>Agent: "You're just a program..."
    Agent->>Thread: Add to history
    Agent->>LLM: Process with full context
    LLM-->>Agent: Response 2
    Agent-->>User: Contextual response
```

### 4. Tool-Calling Agent (Weather)

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant LLM
    participant WeatherTools
    
    User->>Agent: "What's the weather in Madrid?"
    Agent->>LLM: Process with tools
    LLM->>WeatherTools: GetWeather("Madrid")
    WeatherTools-->>LLM: "cloudy with high of 15°C"
    LLM-->>Agent: Formatted weather response
    Agent-->>User: Response
```

### 5. Structured Output (PersonInfo)

```mermaid
sequenceDiagram
    participant User
    participant Agent
    participant LLM
    participant Parser
    
    User->>Agent: "Tell me about Neo from The Matrix"
    Agent->>LLM: Request JSON-structured response
    LLM-->>Agent: Raw JSON response
    Agent->>Parser: Extract and clean JSON
    Parser-->>Agent: PersonInfo object
    Agent-->>User: Name, Age, Occupation
```

### 6. Agent-as-Tool (Spanish Translator + Weather)

```mermaid
sequenceDiagram
    participant User
    participant SpanishAgent
    participant WeatherAgent
    participant LLM
    participant WeatherTools
    
    User->>SpanishAgent: "What is the weather in Madrid?"
    SpanishAgent->>LLM: Process with WeatherAgent as tool
    LLM->>WeatherAgent: AsAIFunction() call
    WeatherAgent->>LLM: Process weather request
    LLM->>WeatherTools: GetWeather("Madrid")
    WeatherTools-->>LLM: Weather data
    LLM-->>WeatherAgent: English response
    WeatherAgent-->>SpanishAgent: Weather info
    SpanishAgent->>LLM: Translate to Spanish
    LLM-->>SpanishAgent: Spanish response
    SpanishAgent-->>User: Response in Spanish
```

## Microsoft Agent Framework

```mermaid
classDiagram
    class ChatClientAgent {
        +Name: string
        +ChatOptions: ChatOptions
        +RunAsync(prompt)
        +GetNewThread()
    }
    
    class AgentWorkflowBuilder {
        +BuildSequential(agents)
    }
    
    class AIAgent {
        +AsAIFunction()
        +RunAsync(prompt, thread)
    }
    
    class AIFunction {
        +Name: string
        +Description: string
        +InvokeAsync(args)
    }
    
    ChatClientAgent --|> AIAgent
    AgentWorkflowBuilder --> AIAgent
    AIAgent --> AIFunction
```

## Demo Descriptions

### 1. GhostWriter Workflow
- **Pattern:** Sequential workflow (AgentWorkflowBuilder)
- **Agents:** Writer → Editor
- **Tools:** `GetAuthor()`, `FormatStory()`
- **Prompts:** StoryWriter.prompty, StoryEditor.prompty
- **Demo:** Two-agent content creation pipeline with tool calling

### 2. Agent Smith Prompt
- **Pattern:** Single prompt
- **Tools:** None
- **Prompt:** AgentSmith.prompty
- **Demo:** Single question/answer with Matrix-themed persona

### 3. Agent Smith Conversation
- **Pattern:** Multi-turn conversation with AgentThread
- **Tools:** None
- **Prompt:** AgentSmith.prompty
- **Demo:** Context-aware conversation using thread history

### 4. Weather Function Calling
- **Pattern:** Tool-calling
- **Tools:** `GetWeather(location)`
- **Prompt:** WeatherAssistant.prompty
- **Demo:** Natural language weather queries with function invocation

### 5. Structured Output (PersonInfo)
- **Pattern:** JSON structured output
- **Tools:** None
- **Prompt:** PersonInfo.prompty
- **Demo:** Parse LLM response into typed PersonInfo object

### 6. Agent-as-Tool (Spanish Translator)
- **Pattern:** Agent composition via AsAIFunction()
- **Tools:** WeatherAgent (as AIFunction)
- **Prompts:** WeatherAssistant.prompty, SpanishTranslator.prompty
- **Demo:** Translator agent uses weather agent as a callable tool

## Project Structure

```
AI.Workshop.Console.Agents/
├── Program.cs                      # Entry point - runs all demos sequentially
├── GhostWriterAgents.cs            # Sequential workflow demo (Writer → Editor)
├── AgentSmithPromptDemo.cs         # Single prompt demo
├── AgentSmithConversationDemo.cs   # Multi-turn conversation demo
├── WeatherFunctionDemo.cs          # Function calling demo
├── StructuredOutputDemo.cs         # JSON structured output parsing
├── AgentAsToolDemo.cs              # Agent-as-tool pattern demo
├── WeatherTools.cs                 # Static weather tool functions
├── PersonInfo.cs                   # PersonInfo model class
├── appsettings.json                # Configuration
└── Prompts/
    ├── AgentSmith.prompty          # Matrix-themed agent persona
    ├── WeatherAssistant.prompty    # Weather agent instructions
    ├── PersonInfo.prompty          # Structured output instructions
    ├── StoryWriter.prompty         # Creative writing agent
    ├── StoryEditor.prompty         # Story editing agent
    └── SpanishTranslator.prompty   # Spanish translation agent
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Microsoft.Agents.AI | 0.8.2 | Agent Framework |
| Microsoft.Extensions.AI | 10.0.1 | AI abstractions |
| Ollama | - | LLM backend (llama3.2) |
| Prompty.Core | 0.2.3 | Prompt templates |
| AI.Workshop.Common | - | Shared utilities, health checks |

## Usage

```bash
cd AI.Workshop.Console.Agents
dotnet run
```

**Demo Sequence:**
The application runs all demos sequentially with "Press any key to continue..." between each:

```
=== Ghost Writer Workflow ===
Configuring Ghost Writer workflow (Writer -> Editor)...
Running workflow prompt: Write a short story about a haunted house.
[Story output]

=== Matrix Agents - Single Prompt ===
Prompting Agent Smith with a single question...
Neo: What is the matrix?
Agent Smith: [Response]

=== Matrix Agents - Multi-turn Conversation ===
Continuing the conversation thread with Agent Smith...
[Multi-turn dialogue]

=== Weather Agent - Function Calling ===
Weather agent will call the GetWeather function to answer the user.
User: What's the weather like in Madrid?
Weather Agent: [Response with tool call]

=== PersonInfo Agent - Structured Output ===
Raw response: {"name": "Neo", "age": 33, "occupation": "Hacker"}
Parsed structured output:
Name: Neo, Age: 33, Occupation: Hacker

=== SpanishTranslator Agent - Agent-as-Tool Workflow ===
Using the translator agent to call the weather agent as a tool...
User: What is the weather in Madrid?
Spanish Translator: [Spanish response]
```
