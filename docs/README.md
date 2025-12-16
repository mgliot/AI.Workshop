# 📐 Architecture Documentation

This folder contains architecture diagrams and technical documentation for each project in the AI.Workshop solution.

## Projects

| Document | Project | Description |
|----------|---------|-------------|
| [Common](./AI.Workshop.Common.md) | AI.Workshop.Common | Shared utilities, DI extensions, TOON, caching |
| [Guardrails](./AI.Workshop.Guardrails.md) | AI.Workshop.Guardrails | AI safety middleware with validators |
| [VectorStore](./AI.Workshop.VectorStore.md) | AI.Workshop.VectorStore | Vector databases and RAG pipelines |
| [Console.VectorDemos](./AI.Workshop.Console.VectorDemos.md) | AI.Workshop.Console.VectorDemos | Vector store demonstrations |
| [Console.AgentChat](./AI.Workshop.Console.AgentChat.md) | AI.Workshop.Console.AgentChat | Interactive RAG agent navigator |
| [Console.Agents](./AI.Workshop.Console.Agents.md) | AI.Workshop.Console.Agents | Microsoft Agent Framework demos |
| [WebApi.Agents](./AI.Workshop.WebApi.Agents.md) | AI.Workshop.WebApi.Agents | REST API with agent workflows |
| [MCP](./AI.Workshop.MCP.md) | MCP Projects | Model Context Protocol implementations |
| [ChatApp (Aspire)](./AI.Workshop.ChatApp.md) | Aspire ChatApp | Full Blazor chat with Aspire orchestration |

## Diagram Legend

All diagrams use [Mermaid](https://mermaid.js.org/) syntax for rendering on GitHub.

```
🟦 Blue boxes = External services/infrastructure
🟩 Green boxes = Application components
🟨 Yellow boxes = Data stores
🟪 Purple boxes = AI/ML components
```
