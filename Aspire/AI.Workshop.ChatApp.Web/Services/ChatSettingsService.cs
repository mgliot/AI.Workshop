using AI.Workshop.Common;
using AI.Workshop.Common.Toon;

namespace AI.Workshop.ChatApp.Web.Services;

/// <summary>
/// Service for managing chat settings that can be toggled by users
/// </summary>
public class ChatSettingsService
{
    /// <summary>
    /// Currently selected agent type
    /// </summary>
    public AgentType SelectedAgent { get; private set; } = AgentType.DocumentSearch;

    /// <summary>
    /// When enabled, validates input/output through guardrails
    /// </summary>
    public bool GuardrailsEnabled { get; private set; } = true;

    /// <summary>
    /// When enabled, formats data as TOON for reduced token usage
    /// </summary>
    public bool ToonEnabled { get; private set; } = true;

    /// <summary>
    /// Tracks token usage across the session (always enabled)
    /// </summary>
    public TokenUsageTracker TokenTracker { get; } = new();

    /// <summary>
    /// Current response stats (updated per response)
    /// </summary>
    public ResponseStats? CurrentStats { get; private set; }

    /// <summary>
    /// Event fired when settings change
    /// </summary>
    public event Action? OnSettingsChanged;

    /// <summary>
    /// Sets the selected agent
    /// </summary>
    public void SetAgent(AgentType agent)
    {
        if (SelectedAgent != agent)
        {
            SelectedAgent = agent;
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Toggles guardrails on/off
    /// </summary>
    public void ToggleGuardrails()
    {
        GuardrailsEnabled = !GuardrailsEnabled;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Toggles TOON format on/off
    /// </summary>
    public void ToggleToon()
    {
        ToonEnabled = !ToonEnabled;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Resets the token tracker
    /// </summary>
    public void ResetTokenTracker()
    {
        TokenTracker.Reset();
        CurrentStats = null;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Updates the current response stats
    /// </summary>
    public void UpdateStats(ResponseStats stats)
    {
        CurrentStats = stats;
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Clears current stats
    /// </summary>
    public void ClearCurrentStats()
    {
        CurrentStats = null;
    }
}

/// <summary>
/// Stats for a single response
/// </summary>
public record ResponseStats(
    TimeSpan ResponseTime,
    long? InputTokens,
    long? OutputTokens,
    long? TotalTokens,
    FormatComparison? ToonComparison)
{
    public string FormatResponseTime() => ResponseTime.TotalSeconds < 1
        ? $"{ResponseTime.TotalMilliseconds:F0}ms"
        : $"{ResponseTime.TotalSeconds:F1}s";

    public string FormatTokens() => TotalTokens.HasValue
        ? $"{InputTokens ?? 0} in / {OutputTokens ?? 0} out = {TotalTokens} total"
        : "N/A";
}
