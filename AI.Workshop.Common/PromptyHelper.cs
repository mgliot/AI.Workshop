using Microsoft.Extensions.AI;

namespace AI.Workshop.Common;

/// <summary>
/// Shared helper class for loading Prompty prompt templates.
/// Projects using this helper should set PromptsDirectory or use the default "Prompts" folder.
/// </summary>
public static class PromptyHelper
{
    private static string? _promptsDirectory;
    private static bool _invokerInitialized;
    private static readonly object _initLock = new();

    /// <summary>
    /// Ensures the Prompty invoker factory is initialized with all available invokers.
    /// </summary>
    private static void EnsureInvokerInitialized()
    {
        if (_invokerInitialized) return;
        lock (_initLock)
        {
            if (_invokerInitialized) return;
            Prompty.Core.InvokerFactory.AutoDiscovery();
            _invokerInitialized = true;
        }
    }

    /// <summary>
    /// Gets or sets the prompts directory. Defaults to "Prompts" folder in the application base directory.
    /// </summary>
    public static string PromptsDirectory
    {
        get => _promptsDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");
        set => _promptsDirectory = value;
    }

    /// <summary>
    /// Loads a Prompty file and returns the system prompt
    /// </summary>
    /// <param name="promptName">Name of the prompty file (without extension)</param>
    /// <returns>The system prompt content, or empty string if not found</returns>
    public static string GetSystemPrompt(string promptName)
    {
        EnsureInvokerInitialized();
        
        var promptyPath = Path.Combine(PromptsDirectory, $"{promptName}.prompty");

        if (!File.Exists(promptyPath))
        {
            throw new FileNotFoundException($"Prompty file not found: {promptyPath}");
        }

        var prompty = Prompty.Core.Prompty.Load(promptyPath);

        // Prepare returns dynamic - iterate to find system message
        dynamic messages = prompty.Prepare(new { });
        foreach (var m in messages)
        {
            // Role can be ChatRole or string depending on Prompty version
            string roleStr = m.Role is ChatRole cr ? cr.Value : m.Role.ToString();
            if (roleStr.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                // ChatMessage uses Text property, not Content
                if (m is ChatMessage chatMsg)
                    return chatMsg.Text ?? string.Empty;
                // Fallback for dynamic access
                return m.Text ?? m.Content ?? string.Empty;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Loads a Prompty file and prepares messages with the given inputs, returning them as ChatMessages
    /// </summary>
    /// <param name="promptName">Name of the prompty file (without extension)</param>
    /// <param name="inputs">Input parameters for the prompt template</param>
    /// <returns>List of ChatMessage objects</returns>
    public static List<ChatMessage> PrepareAsChatMessages(string promptName, object inputs)
    {
        EnsureInvokerInitialized();
        
        var promptyPath = Path.Combine(PromptsDirectory, $"{promptName}.prompty");

        if (!File.Exists(promptyPath))
        {
            throw new FileNotFoundException($"Prompty file not found: {promptyPath}");
        }

        var prompty = Prompty.Core.Prompty.Load(promptyPath);
        dynamic messages = prompty.Prepare(inputs);

        var result = new List<ChatMessage>();
        foreach (var m in messages)
        {
            // Role can be ChatRole or string depending on Prompty version
            string roleStr = m.Role is ChatRole cr ? cr.Value : m.Role.ToString();
            var role = roleStr.ToLowerInvariant() switch
            {
                "system" => ChatRole.System,
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                _ => ChatRole.User
            };
            result.Add(new ChatMessage(role, (string)m.Content));
        }
        return result;
    }
}
