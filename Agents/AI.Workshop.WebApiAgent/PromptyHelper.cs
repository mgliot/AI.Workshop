using Prompty.Core;

namespace AI.Workshop.WebApiAgent;

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
        
        // Prepare returns dynamic - iterate to find system message
        dynamic messages = prompty.Prepare(new { });
        foreach (var m in messages)
        {
            if (m.Role == "system")
                return m.Content ?? string.Empty;
        }
        return string.Empty;
    }
}
