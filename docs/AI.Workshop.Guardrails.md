# AI.Workshop.Guardrails

AI safety middleware library providing input/output validation with configurable validators and actions.

## Architecture

```mermaid
graph TB
    subgraph "Client Application"
        APP[Application Code]
        CC[IChatClient]
    end
    
    subgraph "Guardrails Pipeline"
        GCC[GuardrailsChatClient]
        GS[GuardrailsService]
        
        subgraph "Validator Chain (Priority Order)"
            RL[RateLimitingValidator<br/>Priority: 5]
            IL[InputLengthValidator<br/>Priority: 10]
            PI[PromptInjectionValidator<br/>Priority: 20]
            PII[PiiValidator<br/>Priority: 30]
            TR[TopicRestrictionValidator<br/>Priority: 35]
            TOX[ToxicityValidator<br/>Priority: 40]
            KW[CustomKeywordValidator<br/>Priority: 50]
            LLM[LlmModerationValidator<br/>Priority: 100]
        end
    end
    
    subgraph "Actions"
        BLK[Block]
        LOG[LogOnly]
        RED[Redact]
    end
    
    subgraph "Telemetry"
        MET[GuardrailsMetrics]
        OTEL[OpenTelemetry]
    end
    
    APP --> CC
    CC --> GCC
    GCC --> GS
    GS --> RL --> IL --> PI --> PII --> TR --> TOX --> KW --> LLM
    GS --> MET
    MET --> OTEL
    
    PI -->|Violation| BLK
    PII -->|Violation| RED
    TOX -->|Violation| BLK
```

## Validator Pipeline

```mermaid
sequenceDiagram
    participant User
    participant GuardrailsChatClient
    participant GuardrailsService
    participant Validators
    participant InnerChatClient
    participant LLM
    
    User->>GuardrailsChatClient: SendMessage
    GuardrailsChatClient->>GuardrailsService: ValidateInput(content)
    
    loop Each Validator (by priority)
        GuardrailsService->>Validators: Validate(content)
        alt Violation Found
            Validators-->>GuardrailsService: ViolationResult
            GuardrailsService-->>GuardrailsChatClient: Blocked/Redacted
            GuardrailsChatClient-->>User: Error or Redacted Content
        end
    end
    
    GuardrailsService-->>GuardrailsChatClient: Allowed
    GuardrailsChatClient->>InnerChatClient: SendMessage
    InnerChatClient->>LLM: Generate Response
    LLM-->>InnerChatClient: Response
    InnerChatClient-->>GuardrailsChatClient: Response
    
    GuardrailsChatClient->>GuardrailsService: ValidateOutput(response)
    GuardrailsService-->>GuardrailsChatClient: Allowed/Blocked
    GuardrailsChatClient-->>User: Final Response
```

## Validators

### Prompt Injection Detection

```mermaid
graph LR
    subgraph "Attack Patterns (24+)"
        P1[Ignore previous instructions]
        P2[System prompt override]
        P3[Jailbreak attempts]
        P4[Role manipulation]
        P5[Output format hijacking]
        P6[Encoding tricks]
    end
    
    subgraph "Detection"
        RE[Regex Patterns]
        KW[Keyword Matching]
        CTX[Context Analysis]
    end
    
    P1 & P2 & P3 & P4 & P5 & P6 --> RE & KW & CTX
    RE & KW & CTX --> DEC{Detected?}
    DEC -->|Yes| BLK[Block]
    DEC -->|No| PASS[Pass]
```

### PII Detection

| PII Type | Pattern | Example |
|----------|---------|---------|
| SSN | `\d{3}-\d{2}-\d{4}` | 123-45-6789 |
| Credit Card | `\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}` | 4111-1111-1111-1111 |
| Email | Standard email regex | user@example.com |
| Phone | Multiple formats | (555) 123-4567 |
| IP Address | IPv4 pattern | 192.168.1.1 |
| Passport | Common formats | AB1234567 |

### Topic Restriction

```mermaid
flowchart TB
    subgraph Input
        MSG[User Message]
    end
    
    subgraph "Topic Matching"
        KM[Keyword Matching]
        SM[Semantic Similarity]
        EMB[Embedding Generator]
    end
    
    subgraph "Allowed Topics"
        T1[programming]
        T2[software development]
        T3[technology]
    end
    
    MSG --> KM
    MSG --> EMB --> SM
    SM --> COS[Cosine Similarity]
    COS --> THR{> Threshold?}
    THR -->|Yes| ALLOW[Allow]
    THR -->|No| BLOCK[Block]
```

## Telemetry

```mermaid
graph TB
    subgraph "Metrics Collection"
        GS[GuardrailsService]
        GM[GuardrailsMetrics]
    end
    
    subgraph "OpenTelemetry"
        AS[ActivitySource]
        MT[Meter]
    end
    
    subgraph "Exported Metrics"
        M1[guardrails.validations.total]
        M2[guardrails.validations.blocked]
        M3[guardrails.validations.allowed]
        M4[guardrails.violations]
        M5[guardrails.rate_limit.hits]
        M6[guardrails.validation.duration]
    end
    
    subgraph "Traces"
        T1[guardrails.validate_input]
        T2[guardrails.validate_output]
        T3[guardrails.validator.*]
    end
    
    GS --> GM
    GM --> AS --> T1 & T2 & T3
    GM --> MT --> M1 & M2 & M3 & M4 & M5 & M6
```

## Configuration

```mermaid
graph LR
    subgraph "Options"
        GO[GuardrailsOptions]
    end
    
    subgraph "Detection Flags"
        F1[EnablePromptInjectionDetection]
        F2[EnablePiiDetection]
        F3[EnableToxicityFiltering]
        F4[EnableTopicRestriction]
        F5[EnableRateLimiting]
        F6[EnableLlmModeration]
    end
    
    subgraph "Limits"
        L1[MaxInputLength: 10000]
        L2[MaxOutputLength: 50000]
        L3[RateLimitMaxRequests: 60]
        L4[RateLimitWindowSeconds: 60]
    end
    
    subgraph "Actions"
        A1[Block]
        A2[LogOnly]
        A3[Redact]
    end
    
    GO --> F1 & F2 & F3 & F4 & F5 & F6
    GO --> L1 & L2 & L3 & L4
    GO --> A1 & A2 & A3
```

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime |
| Microsoft.Extensions.AI | 10.0.1 | ChatClient pipeline |
| System.Diagnostics.DiagnosticSource | - | OpenTelemetry activities |

## Usage

```csharp
// Pipeline integration
IChatClient client = new ChatClientBuilder(ollamaClient)
    .UseGuardrails(options =>
    {
        options.EnablePromptInjectionDetection = true;
        options.EnablePiiDetection = true;
        options.DefaultAction = GuardrailAction.Block;
    })
    .Build();

// DI registration
builder.Services.AddGuardrails(options =>
{
    options.EnableToxicityFiltering = true;
    options.BlockedKeywords = ["confidential"];
});

// Access metrics
var metrics = services.GetService<GuardrailsMetrics>();
var summary = metrics.GetSummary();
Console.WriteLine($"Block rate: {summary.BlockRate:F1}%");
```
