namespace AI.Workshop.ConsoleApps.AgentChat;

/// <summary>
/// Settings for chat features that can be toggled interactively
/// </summary>
public class ChatSettings
{
    /// <summary>
    /// When enabled, validates input/output through guardrails
    /// </summary>
    public bool GuardrailsEnabled { get; set; } = false;

    /// <summary>
    /// When enabled, formats data as TOON for reduced token usage
    /// </summary>
    public bool ToonEnabled { get; set; } = false;

    /// <summary>
    /// Gets a status string for display
    /// </summary>
    public string GetStatusDisplay()
    {
        var guardrails = GuardrailsEnabled ? "ON" : "OFF";
        var toon = ToonEnabled ? "ON" : "OFF";
        return $"Guardrails [{guardrails}] | TOON [{toon}]";
    }

    /// <summary>
    /// Gets colored console status
    /// </summary>
    public void PrintStatus()
    {
        Console.Write("  Guardrails [");
        Console.ForegroundColor = GuardrailsEnabled ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(GuardrailsEnabled ? "ON" : "OFF");
        Console.ResetColor();
        Console.Write("] | TOON [");
        Console.ForegroundColor = ToonEnabled ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(ToonEnabled ? "ON" : "OFF");
        Console.ResetColor();
        Console.WriteLine("]");
    }
}
