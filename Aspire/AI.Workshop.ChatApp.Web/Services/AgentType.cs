namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Available agent types for the chat application
/// </summary>
public enum AgentType
{
    DocumentSearch,
    PDFSummarization
}

/// <summary>
/// Agent metadata with descriptions
/// </summary>
public static class AgentMetadata
{
    public static readonly Dictionary<AgentType, AgentInfo> Agents = new()
    {
        { AgentType.DocumentSearch, new("Document Search", "Search documents with detailed citations", "search") },
        { AgentType.PDFSummarization, new("PDF Summarization", "Summarize PDF documents chapter by chapter", "document") }
    };
}

public record AgentInfo(string Name, string Description, string Icon);
