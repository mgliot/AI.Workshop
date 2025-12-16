using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AI.Workshop.MCP.ConsoleClient;

internal class WorkshopMcpService : IAsyncDisposable
{
    private readonly StdioClientTransport _transport;
    private static McpClient? _mcpClient;

    public WorkshopMcpService()
    {
        if (_mcpClient != null)
        {
            _transport = null!;
            return;
        }

        var serverDir = Path.GetFullPath(AppContext.BaseDirectory.Replace("ConsoleClient", "ConsoleServer"));
        var serverDll = Path.Combine(serverDir, "AI.Workshop.MCP.ConsoleServer.dll");

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
