namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Available agent types for the chat application
/// </summary>
public enum AgentType
{
    DocumentSearch,
    PDFSummarization,
    StudyGuide
}

/// <summary>
/// Agent metadata with descriptions
/// </summary>
public static class AgentMetadata
{
    public static readonly Dictionary<AgentType, AgentInfo> Agents = new()
    {
        { AgentType.DocumentSearch, new("Document Search", "Search documents with detailed citations", "search") },
        { AgentType.PDFSummarization, new("PDF Summarization", "Summarize PDF documents chapter by chapter", "document") },
        { AgentType.StudyGuide, new("Study Guide", "Generate exhaustive study guides from complete PDF content", "book") }
    };
}

public record AgentInfo(string Name, string Description, string Icon);
