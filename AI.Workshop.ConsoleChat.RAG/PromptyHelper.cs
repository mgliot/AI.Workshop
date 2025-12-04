using Prompty.Core;

namespace AI.Workshop.ConsoleChat.RAG;

/// <summary>
/// Helper class for loading Prompty prompt templates
/// </summary>
public static class PromptyHelper
{
    private static readonly string PromptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");

    /// <summary>
    /// Loads a Prompty file and returns the system prompt
    /// </summary>
    public static string GetSystemPrompt(string promptName)
    {
        var promptyPath = Path.Combine(PromptsDirectory, $"{promptName}.prompty");
        var prompty = Prompty.Core.Prompty.Load(promptyPath);
        
        // Prepare returns dynamic - cast to IEnumerable<dynamic>
        dynamic messages = prompty.Prepare(new { });
        foreach (var m in messages)
        {
            if (m.Role == "system")
                return m.Content ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Loads a Prompty file and prepares messages with the given inputs, returning them as ChatMessages
    /// </summary>
    public static List<Microsoft.Extensions.AI.ChatMessage> PrepareAsChatMessages(string promptName, object inputs)
    {
        var promptyPath = Path.Combine(PromptsDirectory, $"{promptName}.prompty");
        var prompty = Prompty.Core.Prompty.Load(promptyPath);
        dynamic messages = prompty.Prepare(inputs);
        
        var result = new List<Microsoft.Extensions.AI.ChatMessage>();
        foreach (var m in messages)
        {
            var role = (string)m.Role switch
            {
                "system" => Microsoft.Extensions.AI.ChatRole.System,
                "user" => Microsoft.Extensions.AI.ChatRole.User,
                "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                _ => Microsoft.Extensions.AI.ChatRole.User
            };
            result.Add(new Microsoft.Extensions.AI.ChatMessage(role, (string)m.Content));
        }
        return result;
    }
}
