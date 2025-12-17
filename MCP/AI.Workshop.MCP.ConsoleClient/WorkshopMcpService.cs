using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AI.Workshop.MCP.ConsoleClient;

internal class WorkshopMcpService : IAsyncDisposable
{
    private readonly StdioClientTransport _transport;
    private McpClient? _mcpClient;

    public WorkshopMcpService(AppSettings settings)
    {
        var serverDir = Path.GetFullPath(AppContext.BaseDirectory.Replace("AI.Workshop.MCP.ConsoleClient", settings.McpServer.ServerProject));
        var serverDll = Path.Combine(serverDir, settings.McpServer.ServerDll);

        _transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "MCP Console Client",
            Command = "dotnet",
            Arguments = [serverDll],
            WorkingDirectory = serverDir,
            EnvironmentVariables = new Dictionary<string, string?>
            {
                { "DOTNET_ENVIRONMENT", "Development" },
                { "MCP_LOG_LEVEL", "Debug" }
            }
        });
    }

    public async Task<McpClient> GetClientAsync()
    {
        _mcpClient ??= await McpClient.CreateAsync(_transport);

        return _mcpClient;
    }

    public async Task<IEnumerable<AIFunction>> GetToolsAsync()
    {
        var client = await GetClientAsync();
        return await client.ListToolsAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_mcpClient != null)
        {
            await _mcpClient.DisposeAsync();
            _mcpClient = null;
        }
    }
}
