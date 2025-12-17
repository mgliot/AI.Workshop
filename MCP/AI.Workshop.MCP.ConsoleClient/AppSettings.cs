namespace AI.Workshop.MCP.ConsoleClient;

public class AppSettings
{
    public OllamaSettings Ollama { get; set; } = new();
    public McpServerSettings McpServer { get; set; } = new();
}

public class OllamaSettings
{
    public string Uri { get; set; } = "http://localhost:11434/";
    public string ChatModel { get; set; } = "llama3.2";
}

public class McpServerSettings
{
    public string ServerProject { get; set; } = "AI.Workshop.MCP.ConsoleServer";
    public string ServerDll { get; set; } = "AI.Workshop.MCP.ConsoleServer.dll";
}
